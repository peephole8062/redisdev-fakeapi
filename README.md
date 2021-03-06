![GitHub Logo](https://repository-images.githubusercontent.com/183810162/ee540000-72aa-11e9-944d-647b80365c34)

[![Build Status](https://dev.azure.com/redisdev/FakeApi/_apis/build/status/redisdev.FakeApi?branchName=master)](https://dev.azure.com/redisdev/FakeApi/_build/latest?definitionId=1&branchName=master)

[![NuGet](https://img.shields.io/nuget/v/FakeApi.svg)](https://www.nuget.org/packages/FakeApi/)

[![codecov](https://img.shields.io/badge/coverage-100%25-27c267.svg)](https://dev.azure.com/redisdev/FakeApi/_build/results?buildId=91&view=ms.vss-test-web.build-test-results-tab)

# What is FakeApi?

FakeApi provides the ability to send HttpWebRequest and get HttpWebResponses without a server.

# When use FakeApi?

- You are waiting for your backend colleagues to finish developing the Web Api?
- You need to quickly do a demo of a software and you do not have the time to develop Web Api?
- Or you are writing unit tests and you have to mock HttpWebResponse?

FakeApi is for you!

# Oldest versions documentations

- [v1.0.x](https://github.com/redisdev/FakeApi/tree/v1.0.3)
- [v1.1.x](https://github.com/redisdev/FakeApi/tree/v1.1.1)
- [v1.2.x](https://github.com/redisdev/FakeApi/tree/v1.2.0)

# v1.3.0 Release note

- You can now define several result files for each API. Please read 'How to return data from file?' section.

# How to use FakeApi?

### FakeApi configuration file

First of all add to your project a json configuration file named as you like. 

- #### HttpWebResponse default values configuration

In this  file, you can define default values for all properties available in HttpWebResponse .Net and object than will be use by FakeApi to build responses when you are sending HttpWebRequest. These values will be used by default for all apis that you will configure in this file and must be declare at the root path like bellow:
```json
{
  "defaultDelay": 500,
  "defaultHttpCode": 200,
  "defaultMethod": "GET"
}
```
- #### Web Api configuration

Now, you can define your web api configuration:

```json
"apis": [
    {
      "url": "http://localhost:8080/api/users/{idUser}",
      "responses": [
        {
          "active": 1,
          "delay": 300,
          "httpCode": 200,
          "method": "GET",
          "response": "{ 'firstname': 'john', 'lastname':'doe', 'id': 567 }"
        },
        {
          "active": 1,
          "delay": 300,
          "httpCode": 200,
          "method": "PUT",
          "response": "{ 'firstname': 'johnny', 'lastname':'doedoe', 'id': 567 }"
        },
        {
          "active": 0,
          "delay": 300,
          "httpCode": 200,
          "method": "GET",
          "response": "{ 'firstname': 'bob', 'lastname':'dylan', 'id': 567 }"
        },
        {
          "active": 0,
          "delay": 300,
          "httpCode": 200,
          "method": "PUT",
          "response": "{ 'firstname': 'bobby', 'lastname':'dylann', 'id': 567 }"
        }
      ]
    }
```
For each web api you can set several actives responses. One per pair url/method. At runtime you can switch between responses with changing active property value.
Note that you can use template segment in your url configuration (/{idUser}).
You can also override all default values defined at the root path in each apis configuration.

- #### Organize your configurations files

You can configure FakeApi in a single file. But you can now split your Apis configurations files into multiple files. To do that, just provide all directories where FakeApi can find api configurations files by this way :

```json
{
  "defaultDelay": 250,
  "defaultHttpCode": 200,
  "defaultMethod": "GET",
  "apisDirectories": [
      "Config/Api/User"
  ]
}
```

In summary, I advise you as in the example below :
- declare a main file (api.cfg.json) with all default values that you need and all api configurations files directories
- create one configuration file per api type (usersApi.cfg.json, ordersApi.cfg.json etc...)
- and create one json response file per request (getUserById.json, postUser1.json etc...)

![Config example](https://github.com/redisdev/FakeApi/blob/release_v1.2.0/config.png?raw=true)

/!\ Be careful, directories containing api configuration files (such as usersApi.cfg) must not contain any other files! Otherwise FakeApi will try to deserialize them and an exception will be thrown.

- #### How to return data from file?

Simply set the "file" property into your api configuration:

```json
{
      "url": "http://localhost:8080/api/get-file",
      "responses": [
        {
          "active": 1,
          "method": "GET",
          "httpCode": 200,
          "file": "getResponse.txt"
        }
      ]
    }
```

If you want to return different data each time you call one of your APIs, then you can define several result files :

```json
{
  "url": "https://localhost/api/users?pIndex={0}&pSize={1}",
  "responses": [
    {
      "active": 1,
      "files": [
        "Config/Api/User/Response/searchUsersPage0.json",
        "Config/Api/User/Response/searchUsersPage1.json",
        "Config/Api/User/Response/searchUsersPage2.json"
      ]
    }
  ]
}

```
At the first call the data of the first file will be returned then the second to the second call etc ... This type of configuration can be useful for paged requests.

- #### How to add cookie and header?

You can add cookies and headers into HttpWebResponse:

```json
{
      "url": "http://localhost:8080/api/orders/{orderId}/addresses/{addressId}}",
      "responses": [
        {
          "active": 1,
          "delay": 135,
          "httpCode": 200,
          "cookies": [
            {
              "name": "cookie1",
              "value": "valCookie1",
              "comment": "Comment1",
              "discard": 0,
              "domain": "domain1",
              "expired": 1,
              "expires": "2012-10-12",
              "httpOnly": 1,
              "path": "path1",
              "secure": 1,
              "version": 54
            },
            {
              "name": "cookie2",
              "value": "valCookie2"
            }
          ],
          "headers": [
            {
              "name": "header1",
              "value": "valHeader1"
            },
            {
              "name": "header2",
              "value": "valHeader2  "
            }
          ],
          "response": "{ 'orderId': 345, 'streetAddress':'2762 Highland Drive', 'city': 'Nina' }"
        }
```

- #### How to throw a WebException?

Maybe you will have to test how your code reacts when a web exception is throwing. To do that, you need to set the "webExceptionMessage" property:

```json
{
      "url": "http://localhost:8080/api/ad/{adId}",
      "responses": [
        {
          "active": 1,
          "httpCode": 400,
          "webExceptionMessage": "Invalid ad id"
        }
      ]
}
````

- #### How to throw a custom exception?

You can force FakeApi to throw your own exceptions:

```json
{
      "url": "http://localhost:8080/api/ad/{adId}/custom-error",
      "responses": [
        {
          "active": 1,
          "httpCode": 500,
          "customApiException": 
          {
            "fullTypeName": "App.CustomWebException, App",
            "constructorArgs": [
              "custom exception message"
            ]
          }
        }
      ]
}
```

- #### Create a HttpWebRequest and use FakeApi

Finally you have to create your web request and use the FakeHttpRequester provided by FakeApi to get the corresponding HttpWebResponse:

```csharp

var serviceCollection = new ServiceCollection();

#if DEBUG
serviceCollection.AddScoped<IHttpRequester, FakeHttpRequester>(provider =>
{
    return new FakeHttpRequester("api.cfg.json");
});
#endif

var serviceProvider = serviceCollection.BuildServiceProvider();
var httpRequester = serviceProvider.GetService<IHttpRequester>();

//Get user request
var getUserRequest = (HttpWebRequest)WebRequest.Create("http://localhost:8080/api/users/56");
var getUserResponse = httpRequester.GetResponse(getUserRequest);

using (var stream = new StreamReader(getUserResponse.GetResponseStream()))
{
    var data = stream.ReadToEnd();
    var user = JsonConvert.DeserializeObject<User>(data);
    Console.WriteLine($"json data from {getUserRequest.RequestUri}");
    Console.WriteLine($"Firstname : {user.Firstname} | Lastname : {user.Lastname} | Id : {user.Id}");
}
````
- #### IHttpRequester

In production, replace the FakeHttpRequester implementation by DefaultHttpRequester provided by FakeApi. Or you can also write your own implementation of IHttpRequester interface.

```csharp

/// <summary>
/// Provides method to sending a web request
/// </summary>
public interface IHttpRequester
{
    /// <summary>
    /// Gets the web response for <paramref name="request"/>
    /// </summary>
    HttpWebResponse GetResponse(HttpWebRequest request);

    /// <summary>
    /// Gets the web response async for <paramref name="request"/>
    /// </summary>
    Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request);
}

public class DefaultHttpRequester: IHttpRequester
    {
        public HttpWebResponse GetResponse(WebRequest request)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = request.GetResponse();
            return response as HttpWebResponse;
        }

        public async Task<HttpWebResponse> GetResponseAsync(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = await request.GetResponseAsync();
            return response as HttpWebResponse;
        }
    }

```

#### List of available default HttpWebResponse properties

- int DefaultDelay (simulate server latence)
- int DefaultHttpCode
- long DefaultContentLength
- string DefaultContentType
- bool DefaultIsFromCache
- bool DefaultIsMutuallyAuthenticated
- string DefaultMethod
- string DefaultResponseUri
- string DefaultResponse
- string DefaultStatusDescription
- array DefaultCookies

#### List of available properties for each HttpWebResponse configuration (for overriding default properties)

- int Delay (simulate server latence)
- int HttpCode
- long ContentLength
- string ContentType
- bool IsFromCache
- bool IsMutuallyAuthenticated
- string Method
- string ResponseUri
- string Response
- string StatusDescription
- string WebExceptionMessage
- object CustomApiException
- array Cookies
- array Headers

#### List of available properties for HttpWebResponse cookie

- string name
- string value
- string comment
- bool discard
- string domain
- bool expired
- dateTime expires
- bool httpOnly
- string path
- bool secure
- int version

#### List of available properties for HttpWebResponse header

- string name
- string value

#### Single file Json configuration full example

```json

{
  "defaultDelay": 500,
  "defaultHttpCode": 200,
  "defaultMethod": "GET",
  "apis": [
    {
      "url": "http://localhost:8080/api/users/{idUser}",
      "responses": [
        {
          "active": 1,
          "delay": 300,
          "response": "{ 'firstname': 'john', 'lastname':'doe', 'id': 567 }"
        }
      ]
    },
    {
      "url": "http://localhost:8080/api/orders/{orderId}/addresses/{addressId}}",
      "responses": [
        {
          "active": 1,
          "delay": 135,
          "cookies": [
            {
              "name": "cookie1",
              "value": "valCookie1"
            },
            {
              "name": "cookie2",
              "value": "valCookie2"
            }
          ],
          "headers": [
            {
              "name": "header1",
              "value": "valHeader1"
            },
            {
              "name": "header2",
              "value": "valHeader2  "
            }
          ],
          "response": "{ 'orderId': 345, 'streetAddress':'2762 Highland Drive', 'city': 'Nina' }"
        },
        {
          "active": 0,
          "delay": 5000,
          "httpCode": 203,
          "response": "{ 'orderId': 345, 'streetAddress':'2762 Highland Drive', 'city': 'Nina' }"
        }
      ]
    },
    {
      "url": "http://localhost:8080/api/ad/{adId}",
      "responses": [
        {
          "active": 1,
          "httpCode": 400,
          "webExceptionMessage": "Invalid ad id"
        }
      ]
    },
    {
      "url": "http://localhost:8080/api/ad/{adId}/custom-error",
      "responses": [
        {
          "active": 1,
          "httpCode": 500,
          "customApiException": 
          {
            "fullTypeName": "App.CustomWebException, App",
            "constructorArgs": [
              "custom exception message"
            ]
          }
        }
      ]
    },
    {
      "url": "http://localhost:8080/api/get-file",
      "responses": [
        {
          "active": 1,
          "file": "DownloadFile.txt"
        }
      ]
    }
  ]
}

```

### Project example

You can find a full project example under Example solution folder


