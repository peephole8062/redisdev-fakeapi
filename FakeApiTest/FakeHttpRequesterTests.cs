using System;
using System.IO;
using System.Linq;
using System.Net;
using FakeApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeApiTest
{
    [TestClass]
    public class FakeHttpRequesterTests
    {
        [TestInitialize]
        public void SetUp()
        {
            ConfigIO._config = null;
        }

        [TestCleanup]
        public void TearDown()
        {
            ConfigIO._config = null;
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ShouldMockPropertiesFromDefaultConfig(bool runAsync)
        {
            //Arrange
            var filePath = "apisConfig.json";
            var config = new Config();
            config.DefaultContentLength = 2565121024;
            config.DefaultContentType = "defaultContentType";
            config.DefaultHttpCode = (int)HttpStatusCode.Ambiguous;
            config.DefaultIsFromCache = true;
            config.DefaultIsMutuallyAuthenticated = true;
            config.DefaultMethod = "PUT";
            config.DefaultResponse = "defaultResponse";
            config.DefaultResponseUri = "http://localhost/api/myApi";
            config.DefaultStatusDescription = "defaultStatusDescription";
            config.Apis = new[]
            {
                new ApiConfig
                {
                    Url = "http://localhost/api/products/{idProduct}/something"
                }
            };
            config.DefaultCookies = new[]
            {
                new Cookie("ck1", "ck1Value")
                {

                    Comment = "comment1",
                    CommentUri = new System.Uri("https://localhost/comment"),
                    Discard = true,
                    Domain = "domain1",
                    Expired = true,
                    Expires= DateTime.Now,
                    HttpOnly = true,
                    Path = "path",
                    Port = "\"80\", \"8080\"",
                    Secure = true,
                    Version = 56
                },
                new Cookie("ck1", "ck1Value")
            };
            config.DefaultHeaders = new[]
            {
                new HttpHeader { Name = "header1", Value = "valueHeader1" },
                new HttpHeader { Name = "header2", Value = "valueHeader2" },
            };

            ConfigIO.WriteConfig(config, filePath);

            var requester = new FakeHttpRequester(filePath);
            var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost/api/products/2/something");

            //Act
            var response = runAsync ? requester.GetResponseAsync(webRequest).Result : requester.GetResponse(webRequest);

            //Assert
            Assert.AreEqual(config.DefaultContentType, response.ContentType);
            Assert.AreEqual(config.DefaultMethod, response.Method);
            Assert.AreEqual(config.DefaultResponseUri, response.ResponseUri.AbsoluteUri);
            Assert.AreEqual(config.DefaultStatusDescription, response.StatusDescription);
            Assert.AreEqual(config.DefaultContentLength, response.ContentLength);
            Assert.AreEqual(config.DefaultHttpCode, (int)response.StatusCode);
            Assert.AreEqual(config.DefaultIsFromCache, response.IsFromCache);
            Assert.AreEqual(config.DefaultIsMutuallyAuthenticated, response.IsMutuallyAuthenticated);

            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var content = streamReader.ReadToEnd();
                Assert.AreEqual(config.DefaultResponse, content);
            }

            for (int i = 0; i < config.DefaultCookies.Count(); i++)
            {
                var cookieCfg = config.DefaultCookies.ElementAt(i);
                var cookieResponse = response.Cookies[i];
                Assert.AreEqual(cookieCfg.Name, cookieResponse.Name);
                Assert.AreEqual(cookieCfg.Value, cookieResponse.Value);
                Assert.AreEqual(cookieCfg.Comment, cookieResponse.Comment);
                if (cookieCfg.CommentUri != null)
                {
                    Assert.AreEqual(cookieCfg.CommentUri.ToString(), cookieResponse.CommentUri.ToString());
                }
                Assert.AreEqual(cookieCfg.Domain, cookieResponse.Domain);
                Assert.AreEqual(cookieCfg.Path, cookieResponse.Path);
                Assert.AreEqual(cookieCfg.Port, cookieResponse.Port);
                Assert.AreEqual(cookieCfg.Expired, cookieResponse.Expired);
                Assert.AreEqual(cookieCfg.Secure, cookieResponse.Secure);
                Assert.AreEqual(cookieCfg.Expires, cookieResponse.Expires);
                Assert.AreEqual(cookieCfg.Version, cookieResponse.Version);
            }

            for (int i = 0; i < config.DefaultHeaders.Count(); i++)
            {
                var headerCfg = config.DefaultHeaders.ElementAt(i);
                var headerResponseKey = response.Headers.GetKey(i);
                var headerResponseValue = response.Headers.GetValues(headerResponseKey).SingleOrDefault();

                Assert.AreEqual(headerCfg.Name, headerResponseKey);
                Assert.AreEqual(headerCfg.Value, headerResponseValue);
            }
        }

        [TestMethod]
        public void ShouldMockPropertiesFromApiResponse()
        {
            //Arrange
            var filePath = "apisConfig.json";
            var config = new Config();
            var api = new ApiConfig();
            config.Apis = new[] { api };
            api.Url = "http://localhost/api/products/{idProduct}/something";
            var responseCfg = new HttpResponseMock();
            api.Responses = new[] { responseCfg };
            responseCfg.Active = true;
            responseCfg.Delay = 100;
            responseCfg.ContentLength = 2565121024;
            responseCfg.ContentType = "defaultContentType";
            responseCfg.HttpCode = (int)HttpStatusCode.Ambiguous;
            responseCfg.IsFromCache = true;
            responseCfg.IsMutuallyAuthenticated = true;
            responseCfg.Method = "PUT";
            responseCfg.Response = "defaultResponse";
            responseCfg.ResponseUri = "http://localhost/api/myApi";
            responseCfg.StatusDescription = "defaultStatusDescription";
            responseCfg.Cookies = new[]
            {
                new Cookie("ck1", "ck1Value"),
                new Cookie("ck2", "ck2Value")
            };
            responseCfg.Headers = new[]
            {
                new HttpHeader { Name = "header1", Value = "valueHeader1" },
                new HttpHeader { Name = "header2", Value = "valueHeader2" },
            };

            ConfigIO.WriteConfig(config, filePath);

            var requester = new FakeHttpRequester(filePath);
            var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost/api/products/2/something");
            webRequest.Method = "Put";

            //Act
            var webResponse = requester.GetResponse(webRequest);

            //Assert
            Assert.AreEqual(responseCfg.ContentType, webResponse.ContentType);
            Assert.AreEqual(responseCfg.Method, webResponse.Method);
            Assert.AreEqual(responseCfg.ResponseUri, webResponse.ResponseUri.AbsoluteUri);
            Assert.AreEqual(responseCfg.StatusDescription, webResponse.StatusDescription);
            Assert.AreEqual(responseCfg.ContentLength, webResponse.ContentLength);
            Assert.AreEqual(responseCfg.HttpCode, (int)webResponse.StatusCode);
            Assert.AreEqual(responseCfg.IsFromCache, webResponse.IsFromCache);
            Assert.AreEqual(responseCfg.IsMutuallyAuthenticated, webResponse.IsMutuallyAuthenticated);

            using (var streamReader = new StreamReader(webResponse.GetResponseStream()))
            {
                var content = streamReader.ReadToEnd();
                Assert.AreEqual(responseCfg.Response, content);
            }

            for (int i = 0; i < responseCfg.Cookies.Count(); i++)
            {
                var cookieCfg = responseCfg.Cookies.ElementAt(i);
                var cookieResponse = webResponse.Cookies[i];
                Assert.AreEqual(cookieCfg.Name, cookieResponse.Name);
                Assert.AreEqual(cookieCfg.Value, cookieResponse.Value);
            }

            for (int i = 0; i < responseCfg.Headers.Count(); i++)
            {
                var headerCfg = responseCfg.Headers.ElementAt(i);
                var headerResponseKey = webResponse.Headers.GetKey(i);
                var headerResponseValue = webResponse.Headers.GetValues(headerResponseKey).SingleOrDefault();

                Assert.AreEqual(headerCfg.Name, headerResponseKey);
                Assert.AreEqual(headerCfg.Value, headerResponseValue);
            }
        }

        [TestMethod]
        public void ShoulReturnDataFromFile()
        {
            //Arrange
            var filePath = "apisConfig.json";
            var config = new Config();
            config.DefaultMethod = "PUT";
            var api = new ApiConfig();
            config.Apis = new[] { api };
            api.Url = "http://localhost/api/products/{idProduct}/something";
            var responseCfg = new HttpResponseMock();
            api.Responses = new[] { responseCfg };
            responseCfg.Active = true;
            responseCfg.File = "Files/DownloadFile.txt";

            ConfigIO.WriteConfig(config, filePath);

            var requester = new FakeHttpRequester(filePath);
            var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost/api/products/2/something");
            webRequest.Method = "PUT";

            //Act
            var webResponse = requester.GetResponse(webRequest);

            //Assert
            using (var streamReader = new StreamReader(webResponse.GetResponseStream()))
            {
                var content = streamReader.ReadToEnd();
                Assert.AreEqual(File.ReadAllText("Files/DownloadFile.txt"), content);
            }
        }

        [TestMethod]
        public void ShouldThrowWebException()
        {
            //Arrange
            var filePath = "apisConfig.json";
            var config = new Config();
            config.DefaultMethod = "Post";
            var api = new ApiConfig();
            config.Apis = new[] { api };
            api.Url = "http://localhost/api/products/{idProduct}/something";
            var responseCfg = new HttpResponseMock();
            api.Responses = new[] { responseCfg };
            responseCfg.Active = true;
            responseCfg.WebExceptionMessage = "WebException Message";

            ConfigIO.WriteConfig(config, filePath);

            var requester = new FakeHttpRequester(filePath);
            var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost/api/products/2/something");
            webRequest.Method = "POST";

            //Act
            var exception = Assert.ThrowsException<WebException>(() =>
            {
                requester.GetResponse(webRequest);
            });

            //Assert
            Assert.AreEqual(responseCfg.WebExceptionMessage, exception.Message);
        }

        [TestMethod]
        public void ShouldThrowCustomException()
        {
            //Arrange
            var filePath = "apisConfig.json";
            var config = new Config();
            config.DefaultMethod = "PUT";
            var api = new ApiConfig();
            config.Apis = new[] { api };
            api.Url = "http://localhost/api/products/{idProduct}/something";
            var responseCfg = new HttpResponseMock();
            api.Responses = new[] { responseCfg };
            responseCfg.Active = true;
            responseCfg.CustomApiException = new ApiException
            {
                FullTypeName = "FakeApiTest.CustomWebException, FakeApiTest",
                ConstructorArgs = new object[] { "Custom message" }
            };

            ConfigIO.WriteConfig(config, filePath);

            var requester = new FakeHttpRequester(filePath);
            var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost/api/products/2/something");
            webRequest.Method = "PUT";

            //Act
            var exception = Assert.ThrowsException<CustomWebException>(() =>
            {
                requester.GetResponse(webRequest);
            });

            //Assert
            Assert.AreEqual(responseCfg.CustomApiException.ConstructorArgs.First(), exception.Message);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenRequestIsNull()
        {
            //Arrange
            var requester = new FakeHttpRequester("Files/DownloadFile.txt");

            //Act
            var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            {
                requester.GetResponse(null);
            });

            //Assert
            Assert.AreEqual("request", exception.ParamName);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenConfigSourceIsNull()
        {
            //Act
            var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new FakeHttpRequester(null);
            });

            //Assert
            Assert.AreEqual("configSource", exception.ParamName);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenFileNotExists()
        {
            //Act
            var exception = Assert.ThrowsException<FileLoadException>(() =>
            {
                new FakeHttpRequester("FakePath");
            });

            //Assert
            Assert.AreEqual("File FakePath not exists", exception.Message);
        }

        [TestMethod]
        public void CreateResponseStream_ShouldThrowExceptionWhenFileNotExists()
        {
            //Arrange
            var responseMock = new HttpResponseMock();
            responseMock.File = "FakeFile";

            //Act
            var ex = Assert.ThrowsException<FileLoadException>(() =>
           {
               FakeHttpRequester.CreateResponseStream(null, responseMock);
           });

            //Assert
            Assert.AreEqual("File FakeFile not exists", ex.Message);
        }

        [TestMethod]
        public void CreateResponseStream_ShouldCallGetNextFile()
        {
            //Arrange
            var fakeRequester = new FakeHttpRequester("Files/Config/Api/api.cfg.json");
            var request = WebRequest.Create("https://localhost/api/users?pIndex=0&pSize=2");

            //Act
            var response1 = fakeRequester.GetResponse(request);
            var response2 = fakeRequester.GetResponse(request);

            //Assert
            using (var stream1 = new StreamReader(response1.GetResponseStream()))
            using (var stream2 = new StreamReader(response2.GetResponseStream()))
            {
                Assert.AreEqual(File.ReadAllText("Files/Config/Api/usersApi.cfg.json"), stream1.ReadToEnd());
                Assert.AreEqual(File.ReadAllText("Files/Config/Api/ordersApi.cfg.json"), stream2.ReadToEnd());
            }
        }
    }
}
