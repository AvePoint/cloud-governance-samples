
function GetAccessToken($credential, $resource)
{
	try 
    { 
		$user = $u.UserName
		LogToTxt -Value "Get access token of $user for $resource"
		$upc = new-object Microsoft.IdentityModel.Clients.ActiveDirectory.UserPasswordCredential($user, $u.Password.Copy())
		#If it is Office 365 Germany, change to https://login.microsoftonline.de/common/oauth2/token
		$AuthContext = new-object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext("https://login.microsoftonline.com/common/oauth2/token")
	
		$result = [Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContextIntegratedAuthExtensions]::AcquireTokenAsync($AuthContext, $resource,"1b730954-1685-4b74-9bfd-dac224a7b894", $upc).Result
		$result.AccessToken
	}
	catch
    { 
       LogToTxt -Value "An error occurred while getting access token, details: $_"
    } 
}

function IsAppDeployed($site)
{
    CreateAccessToken
	try 
    { 
		$result = @{}
		$result.IsDeployed = $False
		$tempHeader = @{}
		$tempHeader.Add('Authorization', 'Bearer ' + $accessToken)
		$tempHeader.Add('accept','application/json;odata=verbose')
		$apps = Invoke-RestMethod -Uri "$site/_api/web/getappinstancesbyproductid('BB96D1BC-94DC-4FEC-913A-2F2A8E99540F')" -Method GET -Headers $tempHeader
		if($apps.d.results.Length -gt 0)
		{
			$result.IsDeployed = $True
			$result.Status = $apps.d.results[0].Status
			$result.NeedUpgrade = ($apps.d.results[0].AppPrincipalId.Contains('efbb98fd-e154-42b8-96df-f552bc3a862d'))
		}
		$result
	}
	catch
    { 
       LogToTxt -Value "An error occurred while checking whether app deployed, details: $_"
    } 
}

function GetWebPartIds($content, $manager, $context)
{
	try 
    { 
		$webPartIds = @{}
		$regText = "([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\})"
		$reg = New-Object System.Text.RegularExpressions.Regex($regText)
		$guids = $reg.Matches($content)
		
		foreach ($cid in $guids)
		{
		    if($webPartIds.ContainsValue($cid.Value) -eq $False)
		    {
	            try 
                { 
		            $webPart = $manager.WebParts.GetByControlId("g_" + $cid.Value.Replace('-', '_'))
		            $context.Load($webPart);
		            $context.ExecuteQuery();
		            $webPartIds[$webPart.Id.ToString()] = $cid.Value
                }
                catch
                { 
                    LogToTxt -Value "An error occurred while getting web part by $cid, details: $_"
                }
		    }
		}
		$webPartIds
	}
	catch
    { 
       LogToTxt -Value "An error occurred while getting web part IDs, details: $_"
    } 
}

function GetInstalledAppPart($site) #($token, $site)
{
	try 
    { 
		$context = New-Object -TypeName Microsoft.SharePoint.Client.ClientContext -ArgumentList $site
		$context.Credentials = New-Object -TypeName Microsoft.SharePoint.Client.SharePointOnlineCredentials -ArgumentList $u.UserName,$u.Password

		LogToTxt -Value "Loading installed app part in $site"
		$allWebPartData =@{}
		$allWebPartData.Site = $site
		$allWebPartData.IsAppInstalled = $False
		$allWebPartData.IsAppInstallCompleted = $False
		$allWebPartData.HasSiteInfoCard = $False
		$allWebPartData.HasSiteTimeline = $False
		$allWebPartData.IsPagesLibrary = $False
		$allWebPartData.IsWebPagesLibrary = $False
		$allWebPartData.IsClientSidePage = $False
		$allWebPartData.WikiField = $null
		$allWebPartData.PublishingPageContent = $null
		$allWebPartData.WebPartIds = @{}
		
		$productionId = New-Object -TypeName System.Guid -ArgumentList "bb96d1bc-94dc-4fec-913a-2f2a8e99540f"
		$web = $context.Web
		$apps = $web.GetAppInstancesByProductId($productionId)
		$context.Load($web)
		$context.Load($web.RootFolder)
		$context.Load($apps)
		$context.ExecuteQuery()
		if($apps.Count -eq 0)
		{
			LogToTxt -Value "Cloud Governance Online app is not installed in this site $site"
			$allWebPartData
		}
		
		$allWebPartData.IsAppInstalled = $True
		
		$welcomePageSplitData = $web.RootFolder.WelcomePage.Split('/')
		$allWebPartData.WelcomePage = $web.RootFolder.WelcomePage
		
		$file = $web.GetFileByUrl($web.RootFolder.WelcomePage)
		$context.Load($file)
		$context.ExecuteQuery()
		$list = $file.ListItemAllFields.ParentList
		$item = $file.ListItemAllFields
		$context.Load($list)
		$context.Load($item)
		$manager = $file.GetLimitedWebPartManager([Microsoft.SharePoint.Client.WebParts.PersonalizationScope]::Shared)
		$webParts = $manager.WebParts
		$context.Load($webParts)
		$context.ExecuteQuery()

		if($item.FieldValues['ClientSideApplicationId'] -eq 'b6917cb1-93a0-4b97-a84d-7cf49975d4ec')
		{
			LogToTxt -Value "The Welcome page is client side page $site"
			$allWebPartData.IsClientSidePage = $True
		}
		else
		{
			if($item.FieldValues.ContainsKey('PublishingPageContent'))
			{
				$allWebPartData.PublishingPageContent = $item["PublishingPageContent"]
		        $allWebPartData.WebPartIds = GetWebPartIds -content $allWebPartData.PublishingPageContent -manager $manager -context $context
		    }
			if($item.FieldValues.ContainsKey('WikiField'))
			{
				$allWebPartData.WikiField = $item["WikiField"]
		        $allWebPartData.WebPartIds = GetWebPartIds -content $allWebPartData.WikiField -manager $manager -context $context
			}

			Foreach($webPart in $webParts)
			{
				$context.Load($webPart.WebPart)
		        $webPart.Retrieve('ZoneId')
				$context.ExecuteQuery()

				$manager.WebParts.GetById($webPart.Id).WebPart.ExportMode = [Microsoft.SharePoint.Client.WebParts.WebPartExportMode]::All
				$xml = $manager.ExportWebPart($webPart.Id)
				$context.ExecuteQuery()

				if($xml.Value.Contains('SiteInfoCard'))
				{
					$allWebPartData.HasSiteInfoCard = $True
					$allWebPartData.SiteInfoCard = @{}
					$allWebPartData.SiteInfoCard.Xml = $xml.Value
					$allWebPartData.SiteInfoCard.Id = $webPart.Id
					$allWebPartData.SiteInfoCard.Zone = $webPart.ZoneId
				}
				
				if($xml.Value.Contains('SiteTimeline'))
				{
					$allWebPartData.HasSiteTimeline = $True
					$allWebPartData.SiteTimeline = @{}
					$allWebPartData.SiteTimeline.Xml = $xml.Value
					$allWebPartData.SiteTimeline.Id = $webPart.Id
					$allWebPartData.SiteTimeline.Zone = $webPart.ZoneId
				}
			}
			
			LogToTxt -Value "Get Web Part Data Completed $site"
		}

		$t = $apps[0].Uninstall()
		$context.ExecuteQuery()
		$allWebPartData
	}
	catch
    { 
       LogToTxt -Value "An error occurred while getting installed app part, details: $_"
    } 
}

function RestoreAppPart($data)
{
	try 
    { 
		$allWebPartData = $data
		$context = New-Object -TypeName Microsoft.SharePoint.Client.ClientContext -ArgumentList $allWebPartData.Site
		$context.Credentials = New-Object -TypeName Microsoft.SharePoint.Client.SharePointOnlineCredentials -ArgumentList $u.UserName,$u.Password

		$web = $context.Web
		$context.Load($web)
		$context.Load($web.RootFolder)
		$file = $web.GetFileByUrl($allWebPartData.WelcomePage)
		$context.Load($file)
		$manager = $file.GetLimitedWebPartManager([Microsoft.SharePoint.Client.WebParts.PersonalizationScope]::Shared)
		$webParts = $manager.WebParts
		$context.Load($webParts)
		$context.ExecuteQuery()

		$list = $file.ListItemAllFields.ParentList
		$item = $file.ListItemAllFields
		$context.Load($list)
		$context.Load($item)
		$context.ExecuteQuery()

		if($allWebPartData.HasSiteInfoCard -eq $False -and $allWebPartData.HasSiteTimeline -eq $False)
		{
		    return
		}
		$hasCheckeOut = $False
		if ($item -ne $null)
		{
		    if ($list.ForceCheckout -and $file.CheckOutType -eq [Microsoft.SharePoint.Client.CheckOutType]::None)
		    {
		        #some sc template, need the file checkout first
		        $file.CheckOut()
		        $context.Load($file)
		        $context.ExecuteQuery()
		        $hasCheckeOut = $True
		    }
		}
		$cardwebpartId =''
		$timewebpartId =''
		$wikeField = $allWebPartData.WikiField
		$publishingContent = $allWebPartData.PublishingPageContent
		$zoneId = ''
		if($allWebPartData.HasSiteInfoCard)
		{
		    $webpart = $manager.ImportWebPart($allWebPartData.SiteInfoCard.Xml);
		    $webpartAdded = $manager.AddWebPart($webpart.WebPart, $allWebPartData.SiteInfoCard.Zone, 0)
		    $context.Load($webpartAdded)
		    $context.ExecuteQuery()
		    $cardwebpartId = $webpartAdded.Id
		    $zoneId = $allWebPartData.SiteInfoCard.Zone.ToLower()

		    if($allWebPartData.PublishingPageContent -ne $null)
		    {
		        $publishingContent = $publishingContent.Replace($allWebPartData.WebPartIds[$allWebPartData.SiteInfoCard.Id.ToString()], $cardwebpartId.ToString())
		    }
		    if($allWebPartData.WikiField -ne $null)
		    {
		        $wikeField = $wikeField.Replace($allWebPartData.WebPartIds[$allWebPartData.SiteInfoCard.Id.ToString()], $cardwebpartId.ToString())
		    }
		}
		if($allWebPartData.HasSiteTimeline)
		{
		    $webpart = $manager.ImportWebPart($allWebPartData.SiteTimeline.Xml);
		    $webpartAdded = $manager.AddWebPart($webpart.WebPart, $allWebPartData.SiteTimeline.Zone, 0)
		    $context.Load($webpartAdded)
		    $context.ExecuteQuery()
		    $timewebpartId = $webpartAdded.Id
		    $zoneId = $allWebPartData.SiteTimeline.Zone.ToLower()
		    if($allWebPartData.PublishingPageContent -ne $null)
		    {
		        $publishingContent  = $publishingContent.Replace($allWebPartData.WebPartIds[$allWebPartData.SiteTimeline.Id.ToString()], $timewebpartId.ToString())
		    }
		    if($allWebPartData.WikiField -ne $null)
		    {
		        $wikeField = $wikeField.Replace($allWebPartData.WebPartIds[$allWebPartData.SiteTimeline.Id.ToString()], $timewebpartId.ToString())
		    }
		}

		if ($zoneId -ne 'wpz')
		{
		    if ($item -ne $null)
		    {
		        if ($hasCheckeOut -or $file.CheckOutType -eq [Microsoft.SharePoint.Client.CheckOutType]::None)
		        {
		            $file.CheckIn("", [Microsoft.SharePoint.Client.CheckinType]::MajorCheckIn)
		            $context.ExecuteQuery()
		        }
		        if ($list.EnableMinorVersions)
		        {
		            $file.Publish("")
		            $context.ExecuteQuery()
		        }
		        if ($list.EnableModeration)
		        {
		            $file.Approve("")
		            $context.ExecuteQuery()
		        }
		    }
		    return #such as blog zone id left, no need to insert references,only home.aspx
		}

		if($allWebPartData.PublishingPageContent -ne $null)
		{
		    $item["PublishingPageContent"] =  $publishingContent
		}
		if($allWebPartData.WikiField -ne $null)
		{
		    $item["WikiField"] =  $wikeField
		}

		$item.SystemUpdate()
		$context.ExecuteQuery()

		if ($hasCheckeOut -or $file.CheckOutType -ne [Microsoft.SharePoint.Client.CheckOutType]::None)
		{
		    $file.CheckIn("", [Microsoft.SharePoint.Client.CheckinType]::MajorCheckIn)
		    $context.ExecuteQuery()
		}
		if ($list.EnableMinorVersions)
		{
		    $file.Publish("")
		    $context.ExecuteQuery()
		}
		if ($list.EnableModeration)
		{
		    $file.Approve("")
		    $context.ExecuteQuery()
		}
	}
	catch
    { 
       LogToTxt -Value "An error occurred while restoring app part, details: $_"
    } 
}

function InstallApp($site, $appId)
{
    CreateAccessToken
	try 
    { 
		LogToTxt -Value "Install app part $appId in $site"
		Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps/GetById('$appId')/Install" -Method POST -Headers $header
	}
	catch
    { 
       LogToTxt -Value "An error occurred while installing app part, details: $_"
    } 
}

function UploadApp($site)
{
    CreateAccessToken
	try 
    { 
		LogToTxt -value "Upload AvePoint Cloud Governance app to App Catalog site"
		
		#if the customer is in US GOV data center, please use this URL to download GAO app
		#$appContent = (Invoke-WebRequest -Uri "http://download2.avepoint.com/AvePoint Cloud Governance App File/AvePoint_Cloud_Governance_App_USGov_2.0.0.0.app").content
		
		$appContent = (Invoke-WebRequest -Uri "http://download2.avepoint.com/AvePoint Cloud Governance App File/AvePoint_Cloud_Governance_App_2.0.0.0.app").content
		Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/Add(overwrite=true, url='AvePoint_Cloud_Governance_App_2.0.0.0.app')" -Method POST -Headers $header -Body $appContent
	}
	catch
    { 
       LogToTxt -Value "An error occurred while uploading app part, details: $_"
    } 
}

function GetAppId($site)
{
    CreateAccessToken
	try 
    { 
		$allApps = (Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps" -Method GET -Headers $header)| ConvertTo-Json
		foreach($app in (ConvertFrom-Json $allApps).value)
		{
		    if(($app.AppCatalogVersion -eq "2.0.0.0") -and ($app.Title -eq 'AvePoint Cloud Governance'))
		    {
		        return $app.ID
		    }
		}

		$catalogSite = GetAppCatalogSite -site $site
		LogToTxt -value "The app package is not found from catalog site $catalogSite"
		LogToTxt -value "Begin to upload newest Cloud Governance App to catalog site $catalogSite"
		$result = UploadApp -site $catalogSite

		$allApps = (Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps" -Method GET -Headers $header)| ConvertTo-Json

		foreach($app in (ConvertFrom-Json $allApps).value)
		{
		    if(($app.AppCatalogVersion -eq "2.0.0.0") -and ($app.Title -eq 'AvePoint Cloud Governance'))
		    {
		        return $app.ID
		    }
		}
	}
	catch
    { 
       LogToTxt -Value "An error occurred while getting app ID, details: $_"
    } 
}

function GetAppCatalogSite($site)
{
	try 
    { 
		LogToTxt -Value "Get app catalog site $site"
		$context = New-Object -TypeName Microsoft.SharePoint.Client.ClientContext -ArgumentList $site
		$context.Credentials = New-Object -TypeName Microsoft.SharePoint.Client.SharePointOnlineCredentials -ArgumentList $u.UserName,$u.Password
		$tenantSettings = [Microsoft.SharePoint.Client.TenantSettings]::GetCurrent($context)
		$context.Load($tenantSettings)
		$context.ExecuteQuery()
		$tenantSettings.CorporateCatalogUrl
	}
	catch
    { 
       LogToTxt -Value "An error occurred while getting app catalog site, details: $_"
    } 
}


function CreateAccessToken()
{
	try 
    { 
		LogToTxt -Value "Create access token"
		$temp = ([System.DateTime]::UtcNow - $tokenTime).TotalSeconds
		if($temp -gt 2400)
		{
			$global:accessToken = GetAccessToken -credential $u -resource $siteHost
			$global:header = @{}
			$header.Add('Authorization', 'Bearer ' + $accessToken)
			$header.Add('Accept', 'application/json;odata=nometadata')
			$global:tokenTime = [System.DateTime]::UtcNow
		}
	}
	catch
    { 
       LogToTxt -Value "An error occurred while getting app catalog site, details: $_"
    }
}

function LogToTxt($value)
{
    $temp = [System.DateTime]::UtcNow.ToString() + '    ' +$value

    Write-Host $temp
    Add-Content -Path Log.txt -Value $temp
}

LogToTxt -Value "Begin to upgrade all sites from txt"

$allSites = Get-Content "allsites.txt"
$global:siteHost = $allSites[0].SubString(0, $allSites[0].IndexOf('/', 10))

Add-Type -Path Microsoft.IdentityModel.Clients.ActiveDirectory.dll
Add-Type -Path Microsoft.SharePoint.Client.Runtime.dll
Add-Type -Path Microsoft.SharePoint.Client.dll


$global:u = get-credential

$global:tokenTime = [System.DateTime]::UtcNow.AddDays(-1)

$appId = GetAppId -header $header -site $allSites[0]

$allSitesData = @{}

#Load installed app part in home page, and uninstall old version app.
foreach($site in $allSites)
{
    LogToTxt -Value "Start to upgrade $site"
    $appDeployData = IsAppDeployed -site $site

    if($appDeployData.NeedUpgrade)
    {
        $tempData = GetInstalledAppPart -site $site
        $allSitesData[$site] = $tempData

        LogToTxt -Value ("Get App Installed Data Completed for  " + $site + "  " + ($tempData|ConvertTo-Json))
    }
    else
    {
        LogToTxt -Value "No need to upgrade $site"
    }
}

#Upgrade all site app completed.
while($allSitesData.Count -ne 0)
{
    foreach($site in $allSites)
    {
        if($allSitesData.ContainsKey($site))
        {
            $appDeployData = IsAppDeployed -site $site

            if($appDeployData.IsDeployed -eq $False)
            {
                #Waiting for uninstalling old version app completed, and then install new version app
                LogToTxt -Value "Begin to install newest app $site"
                InstallApp -site $site -appId $appId
            }
            else
            {
                if($appDeployData.Status -eq 5)
                {
                    $allWebPartData = $allSitesData[$site]
                    #Waiting for new installing version app completed.
                    if($allWebPartData.IsClientSidePage -eq $False)
                    {
                        LogToTxt -Value "Begin to restore app part for $site"
                        RestoreAppPart -data $allWebPartData
                        LogToTxt -Value "Upgrade Completed for $site"
                    }

                    $allSitesData.Remove($site)
                }
            }
        }
    }

    Start-Sleep -Seconds 10
}

LogToTxt -Value "Upgrade all sites completed."