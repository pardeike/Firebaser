# Firebaser

Nuget package: https://www.nuget.org/packages/Firebaser

### A lightweight mono 3.5 compatible connector to the legacy firebase api.

## About Firebase

Since Firebase became part of the overall Google Cloud Services, it has split into three different database types:  

- Cloud Firestore
- Storage
- Realtime Database  

The original Firebase is now called **Realtime Database** and has support for many modern client SDKs and languages. Unfortunately, development in C# with mono isn't supported except with Unity modules. If you are a low level mod developer, this is not an option plus that most tools for serializing/deserializing are not supported under mono an .NET 3.5.

Firebaser has **no dependencies** and uses the old lecacy REST API that is still supported. Currently, it is mainly for public and shared game data since a lot of authentication mechanics require modern APIs to talk to hardened ssl servers and implementations are hard to find.

Firebaser uses the Firebase Realtime Database and in particular its [REST API](https://firebase.google.com/docs/database/rest/start). There are only two things you need to configure it:  

- the unique part of the firebase hostname
- the auth key for the service account of that instance

## Setting up a new database

First, create yourself an account at [https://firebase.google.com](https://firebase.google.com)  

Next, go to the [Console](https://console.firebase.google.com/) and add a new project. The project name could be anything but be sure to set up or edit the **Project ID** - it will be one of the string you use to configure Firebaser.

Then, authentication needs to be set up. Click on the little gear to the right of `Project Overview` and choose `Project Settings`. Then go to `Service Accounts` and under Legacy Credentials, select `Database secrets`. Click on the `Show` button and make a note of the auth key it reveals. That is the second part of the Firebaser configuration.

Finally, choose `Database` in the main menu and scroll until you see `Realtime Database`. Create one by clicking the button there. While doing so, choose `Start in test mode`.

### Database design

Once the database is created, you are good to go and you can use the resulting console view to edit and watch changes in realtime. Firebase is **schemaless** and creates subnodes on the fly if you use object paths that do not exist yet.

All keys must follow the following rule: `If you create your own keys, they must be UTF-8 encoded, can be a maximum of 768 bytes, and cannot contain., $, #, [, ], /, or ASCII control characters 0-31 or 127. `. Since Firebaser maps json nodes to fields in your C# objects, you need to make sure that those field names match these rules.

### API documentation

The Realtime Database REST API is documented [here](https://firebase.google.com/docs/database/rest/start). Firebaser maps its methods onto the different HTTP methods described in the API. Instead of dealing with json, it serializes and deserializes class objects you declare or have.

One handy thing to remember is that since Firebase uses a node model that maps onto json structures, you can add a complex object at any point in the tree and still use the resulting paths to access and edit the values with the same operation you used to generate the node but for its details.

## Firebaser

Firebaser has a very simple API. Here is a simple fetch operation

```cs
import Firebaser;

public class MyClass  
{  
	var int Count;  
	var string Username;  
}

var databaseIdentifier = "test-firebaser";
var authToken = "6Fw1CwwMfXqmuPtjHIPs0dTBH8AXGPksp5gyyTG4";  
var client = new Connector(databaseIdentifier, authToken);  
var myInstance = client.Get<MyClass>("/MyTopic/Topic1");
```

which would match the following node structure:

```
test-firebaser.firebaseio.com  
+- MyTopic  
   +- Topic1  
      +- Count  
      +- Username  
```

---
## Firebaser API

### Constructor
```cs
public Connector(string project, string secret)
```  
> **Arguments**  
> `project` - name of your database (before ".firebaseio.com")  
> `secret` - the auth token from the lecacy service account  

> **Returns**  
> A connector instance that is used to execute commands to the REST API.

### Checking for an internet connection

```cs
public bool IsAvailable(bool forceCheck = false)
```

Example  
```cs
var databaseIdentifier = "test-firebaser";
var authToken = "6Fw1CwwMfXqmuPtjHIPs0dTBH8AXGPksp5gyyTG4";
var client = new Connector(databaseIdentifier, authToken);
var status = client.IsAvailable();

// Forcing an update:
var status2 = client.IsAvailable(true);
// Note: The internal status will only be updated every 2 seconds
// so passing `true` in `forceCheck` will force a status update.
// This will definitely add an extra connection request so try not
// to spam that call too much
```  

### Sending REST commands
```cs
public TResult Send<TObject, TResult>(Method method, string objectPath, TObject obj = default(TObject), bool shallow = false, NameValueCollection queryParams = null)
```  
> **Arguments**  
> `method` - enum `Method`, defines the HTTP method to use  
> `objectPath` - the path from the root to the object to be read or written
> `obj` - the object to read or write  
> `shallow` - if set to true, replaces all subnodes with `true`  
> `queryParams` - one or more key/value pairs that will be part of the query string  

> **Returns**  
> The resulting object of type \<TResult> or `null` in case of a connection error

### Convenience methods

```cs  
// Getting objects from the database:
public TResult Get<TResult>(string objectPath, bool shallow = false, NameValueCollection queryParams = null)
```

```cs
// Creating new objects by create random node keys:
public string Post<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null)
```

```cs
// Inserting new objects with a given node key:
public string Put<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null)
```

```cs
// Updating a specifc value in the object tree:
public string Patch<TObject>(string objectPath, TObject obj, NameValueCollection queryParams = null)
```

```cs
// Removing an object:
public string Delete(string objectPath, NameValueCollection queryParams = null)
```

## TODO

For now, there is no error handling. If you have no internet connection, all methods return `null` (or the default value for the type).


---

MIT License  
Author: Andreas Pardeike  
*Based on the work of https://github.com/mgholam/fastJSON and modified to work under mono 3.5*
