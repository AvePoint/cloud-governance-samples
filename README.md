# <center>![Cloud Governance](https://raw.githubusercontent.com/AvePoint/cloud-governance-samples/master/cloud-governance.png)AvePoint Cloud Governance</center>

## About Cloud Governance 

[AvePoint Cloud Governance](https://www.avepointonlineservices.com) is an industry leading business automation SAAS platform which focus on the automation of Microsoft Office 365 platform. Over the past few years, a lot of big enterprises in the world integrated their key business solutions with Cloud Governance platform. In order to help the entities easily go though the integration process, we lanuched the [cloud-governance-sample](https://github.com/AvePoint/cloud-governance-samples) project to demostrate the coding details. 

## Project Details
The sample solution shows the possible approachs to interact with [AvePoint Cloud Governance](https://www.avepointonlineservices.com/). Basically the demo solution show three aspects of the AvePoint Cloud Governace. 

* **_sdk_** project shows the Cloud Governance Api(in api client short as GaoApi) demo is conformance with cloud governance web api, please refer to the [Cloud Governance Api guide](https://avepointcdn.azureedge.net/assets/webhelp/avepoint-cloud-governance-api/Index.html "Cloud Governance Api guide") for more details. sdk is a proxy of the web api and give the client side developer a clear vision of the api interface.
* **_custom-action_** project shows a simple custom-action Asp.Net web service which should be treated as outgoing webhook, with the hook service, the end-user has the ability of plugging their customer process in the Cloud Governance workflow process.
* **_tool_** project contains some demo tools for helping end-users automate some tasks in their Office 365 environment.

## Contribute

There are many ways to contribute to the Project.
* [Submit bugs](https://github.com/AvePoint/cloud-governance-samples/issues) and help us verify fixes as they are checked in.
* Review the [source code changes](https://github.com/AvePoint/cloud-governance-samples/pulls).
* Engage with other Cloud Governance users and developers on [AvePoint Online Services Community](https://www.avepoint.com/community/discussion/forum/avepoint-product-forum/online-service/). 

## Documentation

*  [Cloud Governance Web API](https://avepointcdn.azureedge.net/assets/webhelp/avepoint-cloud-governance-api/Index.html)


## Building projects

In order to use the Cloud-Governance-Sample solution, ensure that you have [visual studio 2017](https://www.visualstudio.com/) and [.NET Frameworkd 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49982) installed.

Clone a copy of the repo:

```bash
git clone https://github.com/AvePoint/cloud-governance-samples.git
```
Open the cloud-governance-samples directory and double click the vs solution file.


## Usage

* **_sdk_** project is a demo project of the [cloud-governance-sdk](https://www.nuget.org/packages/cloud.governance.sdk/), you could find this project is a xunit project and every api sample is demostrated in a unit test case.
* **_tool_** project is a hub store location for the sample tools which can help Cloud Governance end-users do the specific job or fix some issues.
* **_custom-action_** project is a demo project for building a customized web service using Asp.Net web service, this service action will be hooked by Cloud Governance worker cluster in the running time.

## Roadmap

For details on our planned demo projects and future direction please refer to our [roadmap](https://github.com/AvePoint/cloud-governance-samples/wiki/Roadmap).
