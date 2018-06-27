function LogToTxt($value)
{
    $temp = [System.DateTime]::UtcNow.ToString() + '    ' +$value

    Write-Host $temp
    Add-Content -Path Log.txt -Value $temp
}

function GetAccessToken($resource)
{
    LogToTxt -Value "Get Access token of $user for $resource"
    $upc = new-object Microsoft.IdentityModel.Clients.ActiveDirectory.UserPasswordCredential($global:username, $global:password.Copy())
	#If it is Office 365 Germany, change to https://login.microsoftonline.de/common/oauth2/token
    $AuthContext = new-object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext("https://login.microsoftonline.com/common/oauth2/token")

    $result = [Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContextIntegratedAuthExtensions]::AcquireTokenAsync($AuthContext, $resource,"1b730954-1685-4b74-9bfd-dac224a7b894", $upc).Result
    $result.AccessToken
}

function CreateAccessToken()
{
    $temp = ([System.DateTime]::UtcNow - $tokenTime).TotalSeconds
    if($temp -gt 2400)
	{
		$global:accessToken = GetAccessToken -resource $siteHost
		$global:header = @{}
		$header.Add('Authorization', 'Bearer ' + $accessToken)
		$header.Add('Accept', 'application/json;odata=nometadata')
		$global:tokenTime = [System.DateTime]::UtcNow
	}
}

function GetAppId($site)
{
    CreateAccessToken
    $allApps = (Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps" -Method GET -Headers $header)| ConvertTo-Json

    foreach($app in (ConvertFrom-Json $allApps).value)
    {
        if(($app.AppCatalogVersion -eq "2.0.0.0") -and ($app.Title -eq 'AvePoint Cloud Governance'))
        {
            return $app.Id
        }
    }

    $catalogSite = GetAppCatalogSite -site $site
    LogToTxt -value "The app package is not found from catalog site $catalogSite"
    LogToTxt -value "Begin to upload newest Cloud Governance App to catalog site $catalogSite"
    UploadApp -site $catalogSite

    $allApps = (Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps" -Method GET -Headers $header)| ConvertTo-Json
    foreach($app in (ConvertFrom-Json $allApps).value)
    {
        if(($app.AppCatalogVersion -eq "2.0.0.0") -and ($app.Title -eq 'AvePoint Cloud Governance'))
        {
            return $app.Id
        }
    }
}

function GetAppCatalogSite($site)
{
    $context = New-Object -TypeName Microsoft.SharePoint.Client.ClientContext -ArgumentList $site
    $context.Credentials = New-Object -TypeName Microsoft.SharePoint.Client.SharePointOnlineCredentials -ArgumentList $global:userName,$global:password
    $tenantSettings = [Microsoft.SharePoint.Client.TenantSettings]::GetCurrent($context)
    $context.Load($tenantSettings)
    $context.ExecuteQuery()
    $tenantSettings.CorporateCatalogUrl
}

function UploadApp($site)
{
    CreateAccessToken
    LogToTxt -value "Upload AvePoint Cloud Governance app to App Catalog site"
	
	#if the customer is in US GOV data center, please use this URL to download GAO app
    #$appContent = (Invoke-WebRequest -Uri "http://download2.avepoint.com/AvePoint Cloud Governance App File/AvePoint_Cloud_Governance_App_USGov_2.0.0.0.app").content
	
    $appContent = (Invoke-WebRequest -Uri "http://download2.avepoint.com/AvePoint Cloud Governance App File/AvePoint_Cloud_Governance_App_2.0.0.0.app").content
    $result = Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/Add(overwrite=true, url='AvePoint_Cloud_Governance_App_2.0.0.0.app')" -Method POST -Headers $header -Body $appContent
}

function UninstallApp($site, $appId)
{
    CreateAccessToken
    Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps/GetById('$appId')/Uninstall" -Method POST -Headers $header
}

function InstallApp($site, $appId)
{
    CreateAccessToken
    Invoke-RestMethod -Uri "$site/_api/web/tenantappcatalog/AvailableApps/GetById('$appId')/Install" -Method POST -Headers $header
}

function IsAppDeployed($site)
{
    CreateAccessToken

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

function RemoveAppParts($site)
{
    $context = New-Object -TypeName Microsoft.SharePoint.Client.ClientContext -ArgumentList $site
    $context.Credentials = New-Object -TypeName Microsoft.SharePoint.Client.SharePointOnlineCredentials -ArgumentList $global:username,$global:password

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
    $allWebPartData.HasApppPanel = $False

    #$context = New-Object -TypeName AvePoint.SP.AveClientContext -ArgumentList $site,$token
	
    $productionId = New-Object -TypeName System.Guid -ArgumentList "bb96d1bc-94dc-4fec-913a-2f2a8e99540f"
    $web = $context.Web
	$apps = $web.GetAppInstancesByProductId($productionId)
    $context.Load($web)
    $context.Load($web.RootFolder)
    $context.Load($apps)
    $context.ExecuteQuery()
	
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
        Connect-PnPOnline -Url $site -Credentials $global:credential
        $pageName = $welcomePageSplitData[$welcomePageSplitData.Length - 1]

        $sidePage = Get-PnPClientSidePage -Identity $pageName

		$needAdd = $True	
		$pageHasChange = $False	
		for($i=$sidePage.Controls.Count;$i -ge 0;$i--)
		{
			$control = $sidePage.Controls[$i]
			if($control.Title -eq 'Site Governance Panel')
            {
                LogToTxt -Value "The card has been added in $site"
                $needAdd = $False
            }
			if($control.Title -eq 'Site Information Card' -or $control.Title -eq 'Site Lifecycle Timeline')
            {
				LogToTxt -Value "Delete WebPart $control.Title in $site"
				$sidePage.Controls.RemoveAt($i)
				$control.Section.Page.Save()
				$pageHasChange = $True
            }
		}

		#$sidePage.Save()
		if ($needAdd -eq $True)
		{       
			LogToTxt -Value "Add Site Governance Panel in $site"
			Add-PnPClientSideWebPart -Page $pageName -Component 'Site Governance Panel'
			$pageHasChange = $True
		}
		
		if ($pageHasChange -eq $True)
		{       
			$sidePage.Publish()
		}		

    }
	else
	{
        $hasCheckeOut = $False

        $infoCardId = $null
        $timelineId = $null
        $hasApppPanel = $False
        $zoneId = 'wpz'

		Foreach($webPart in $webParts)
		{
            $context.Load($webPart.WebPart)
            $webPart.Retrieve('ZoneId')
			$context.ExecuteQuery()
            $zoneId = $webPart.ZoneId

			$manager.WebParts.GetById($webPart.Id).WebPart.ExportMode = [Microsoft.SharePoint.Client.WebParts.WebPartExportMode]::All
			$xml = $manager.ExportWebPart($webPart.Id)
			$context.ExecuteQuery()

			if($xml.Value.Contains('SiteInfoCard'))
			{
                $infoCardId = $webPart.Id
                continue
			}
			
			if($xml.Value.Contains('SiteTimeline'))
			{
                $timelineId = $webPart.Id
                continue
			}

            if($xml.Value.Contains('92daea1d-4111-45fd-bb91-c65a19314037'))
			{
				$hasApppPanel = $True
			}
		}


        if($infoCardId -ne $null -or $timelineId -ne $null -or $hasApppPanel -eq $False)
        {
            if ($list.ForceCheckout -and $file.CheckOutType -eq [Microsoft.SharePoint.Client.CheckOutType]::None)
            {
                #some sc template, need the file checkout first
                $file.CheckOut()
                $context.Load($file)
                $context.ExecuteQuery()

                $manager = $file.GetLimitedWebPartManager([Microsoft.SharePoint.Client.WebParts.PersonalizationScope]::Shared)
                $webParts = $manager.WebParts
                $context.Load($webParts)
                $context.ExecuteQuery()

                $hasCheckeOut = $True
            }

            if($infoCardId -ne $null)
            {
                LogToTxt -Value "Remove Site info card app part for $site"

                $manager.WebParts.GetById($infoCardId).DeleteWebPart()
                $context.ExecuteQuery()
            }

            if($timelineId -ne $null)
            {
                LogToTxt -Value "Remove timeline app part for $site"
                $manager.WebParts.GetById($timelineId).DeleteWebPart()
                $context.ExecuteQuery()
            }
		

            if($hasApppPanel -eq $False)
            {
                LogToTxt -Value "Begin to Add Panel app part in $site"

                $webpart = $manager.ImportWebPart($panelXml);
                $webpartAdded = $manager.AddWebPart($webpart.WebPart, $zoneId, 0)
                $context.Load($webpartAdded)
                $context.ExecuteQuery()
                $panelId = $webpartAdded.Id

                $item = $file.ListItemAllFields
                $context.Load($item)
                $context.ExecuteQuery()

                $html = "<div class='ms-rtestate-read ms-rte-wpbox' style='display:inline-block;margin-left:20px;'>
                        <div class='ms-rtestate-read $panelId' id='div_$panelId'></div>
                        <div style='display:none' id='vid_$panelId'></div>
                      </div>"

                if($item.FieldValues.ContainsKey('PublishingPageContent'))
                {
                    LogToTxt -Value "Begin to Update PublishingPageContent for home page of $site"
                    $item["PublishingPageContent"] = $html + $item["PublishingPageContent"]
                    $item.SystemUpdate()
                    $context.ExecuteQuery()
                }

                if($item.FieldValues.ContainsKey('WikiField'))
                {
                    LogToTxt -Value "Begin to Update WikiField for home page of $site"
                    $value = $item["WikiField"].ToString()
                    $tempIndex = $value.IndexOf('<div class="ms-rte-layoutszone-inner"',[System.StringComparison].Ordinal);
                    if ($tempIndex -ne -1)
                    {
                        $tempValue = $value.Substring($tempIndex)
                        $tabEndIndex = $tempValue.IndexOf(">");
                        $value = $value.Insert($tempIndex + $tabEndIndex + 1, $html)
                        $item["WikiField"] = $value;
                        $item.SystemUpdate()
                        $context.ExecuteQuery()
                    }
                }
            }

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
	}
}


LogToTxt -Value "Begin to upgrade all sites from txt"
$allSites = Get-Content "allsites.txt"
$global:panelXml = Get-Content "Panel App Part.xml"
				
$ProxyIp = "172.28.0.2"
$ProxyPort = "56789"
$proxycredential = $Host.ui.PromptForCredential("Need credentials", "Please enter your proxy user name and password.", "", "")
[system.net.webrequest]::DefaultWebProxy = New-Object system.net.webproxy($ProxyIp, $ProxyPort)
[system.net.webrequest]::DefaultWebProxy.credentials = $proxycredential.GetNetworkCredential()

$global:siteHost = $allSites[0].SubString(0, $allSites[0].IndexOf('/', 10))
Add-Type -Path Microsoft.IdentityModel.Clients.ActiveDirectory.dll
Add-Type -Path Microsoft.SharePoint.Client.Runtime.dll
Add-Type -Path Microsoft.SharePoint.Client.dll

$global:credential = $Host.ui.PromptForCredential("Need credentials", "Please enter your SharePoint user name and password.", "", "")
$global:username = $global:credential.GetNetworkCredential().username
$global:password = ConvertTo-SecureString $global:credential.GetNetworkCredential().password -AsPlainText -Force
$global:tokenTime = [System.DateTime]::UtcNow.AddDays(-1)

$appId = GetAppId -header $header -site $allSites[0]

$allSitesData = @{}

foreach($site in $allSites)
{
    $allSitesData[$site] = $True
    LogToTxt -Value "Check app status in $site"

    $appDeployData = IsAppDeployed -site $site

    if($appDeployData.IsDeployed -eq $False)
    {
        LogToTxt -Value ("Begin to install app for $site")
        InstallApp -site $site -appId $appId
    }

    if($appDeployData.NeedUpgrade -and $appDeployData.Status -eq 5)
    {
        LogToTxt -Value ("Uninstall GAO app in $site")
        UninstallApp -site $site -appId $appId
    }
}

while($allSitesData.Count -ne 0)
{
    foreach($site in $allSites)
    {
        if($allSitesData.ContainsKey($site))
        {
            $tempData = IsAppDeployed -site $site

            if($tempData.Status -eq 5 -and $tempData.NeedUpgrade -eq $False)
            {
                LogToTxt -Value ("Begin to remove app part and add panel $site")
                RemoveAppParts -site $site
                $allSitesData.Remove($site)
                LogToTxt -Value ("End to remove app part and add panel $site")
                continue
            }

            if($tempData.IsDeployed -eq $False)
            {
                InstallApp -site $site -appId $appId
                continue
            }
        }
    }

    Start-Sleep -Seconds 3
}