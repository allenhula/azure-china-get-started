---
services: batch
platforms: python
author: msonecode
---

# Create Azure Batch PaaS Cloud Service by Python

## Introduction

A get-started Azure Batch sample written in Python, using Cloud Service Configuration for computing nodes in Batch pool.
<br/>
<br/>
<br/>

## Prerequisites

*__Python Tools for Visual Studio__*

Install Python Tools for Visual Studio.

http://aka.ms/ptvs
<br/>
<br/>

*__Python SDK__*

Python Recommend the latest 3.5.2 version

https://www.python.org/downloads/
<br/>
<br/>

*__Azure Storage and Azure Batch Python packages__*

Install packages with the below commands in cmd.

```python
py -m pip install -U pip
py -m pip install cryptography
py -m pip install azure-batch
py -m pip install azure-storage
```
<br/>
<br/>

*__Azure Batch account__*

Once you have an Azure subscription, create an Azure Batch account.

https://docs.microsoft.com/en-us/azure/batch/batch-account-create-portal
<br/>
<br/>

*__Azure Storage account__*

Create an Azure Storage account.

https://docs.microsoft.com/en-us/azure/storage/storage-create-storage-account#create-a-storage-account

<br/>
<br/>
<br/>

## Build the Sample

Prepare the information at below from Azure portal.

*__A batch account__*

•	The batch account name

•	The batch account key

•	The batch account URL

*__A storage account__*

•	The storage account name

•	The storage account key

Replace the following properties in file python_tutorial_client.py with your actual values:

`_BATCH_ACCOUNT_NAME = ''`

`_BATCH_ACCOUNT_KEY = ''`

`_BATCH_ACCOUNT_URL = ''`

`_STORAGE_ACCOUNT_NAME = ''`

`_STORAGE_ACCOUNT_KEY = ''`

<br/>
<br/>
<br/>

## Running the Sample

Execute python_tutorial_client.py and see the output displayed as below.

![Execute  output](Images/a.png?raw=true)
