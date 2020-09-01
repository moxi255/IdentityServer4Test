# .net core IdentityServer4  使用客户端凭据保护API
### 1.准备IdentityServer4.Templates模板
1.1 CMD模式下运行  
`dotnet new -i IdentityServer4.Templates`  
1.2 选择文件运行  
`dotnet new is4empty -n IdentityServer`  
1.3 加入Visual Studio 支持
```csharp
cd ..
dotnet new sln -n Quickstart
```  
1.4 加入对应的IdentityServer  
`dotnet sln add .\IdentityServer\IdentityServer.csproj`  
### 2.修改IdentityServer4项目文件
2.1 首先修改Config.cs文件
 ```csharp
public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    new IdentityResource[]
    {
        new IdentityResources.OpenId()
    };
    //定义如下API
    public static IEnumerable<ApiScope> ApiScopes =>
    new List<ApiScope>
    {
        new ApiScope("api1", "My API")
    };
    //定义客户端账户client 密码secret 运行接口范围api1
    public static IEnumerable<Client> Clients =>
    new List<Client>
    {
        new Client
        {
        ClientId = "client",

        // no interactive user, use the clientid/secret for authentication
        AllowedGrantTypes = GrantTypes.ClientCredentials,

        // secret for authentication
        ClientSecrets =
        {
        new Secret("secret".Sha256())
        },

        // scopes that client has access to
        AllowedScopes = { "api1" }
        }
    };
}
```
2.2 项目端口修改为https://localhost:5001 修改项目下面的Properties文件夹下的launchSettings.json文件

```csharp
{
  "profiles": {
    "SelfHost": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5001"
    }
  }
}
```
2.3 接着运行项目输入https://localhost:5001/.well-known/openid-configuration浏览器可看到下面界面
![访问结果](https://img-blog.csdnimg.cn/20200901145042187.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3FxXzM0NTYxODc1,size_16,color_FFFFFF,t_70#pic_center)
### 3.加入API项目并加入到sln中
3.1 创建API项目  
`dotnet new web -n Api`
3.2 修改应用运行端口为6001修改项目下面的Properties文件夹下的launchSettings.json文件

```csharp
  "applicationUrl": "https://localhost:6001;http://localhost:6000"
```
3.3 创建一个接口
```csharp
[Route("identity")]
[Authorize]
public class IdentityController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
    }
}
```
3.4 修改Startup文件
```
services.AddAuthentication("Bearer1")
.AddJwtBearer("Bearer1", options =>
{
    //该地址为认证的地址
    options.Authority = "https://localhost:5001";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false
    };
});
```
```
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```
3.5 访问https://localhost:6001/identity 
访问这个会发现无法访问
![访问该链接](https://img-blog.csdnimg.cn/20200901150225382.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3FxXzM0NTYxODc1,size_16,color_FFFFFF,t_70#pic_center)
访问内置接口
![访问内置接口](https://img-blog.csdnimg.cn/20200901150225365.png?x-oss-process=image/watermark,type_ZmFuZ3poZW5naGVpdGk,shadow_10,text_aHR0cHM6Ly9ibG9nLmNzZG4ubmV0L3FxXzM0NTYxODc1,size_16,color_FFFFFF,t_70#pic_center)
### 4. 加入Client项目
4.1. 加入Client项目并加入到sln中
`dotnet new console -n Client`
4.2 安装外部包
`dotnet add package IdentityModel`
4.3 主函数中进行接口访问
```
var client = new HttpClient();
//访问认证接口获取token
var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    
    return;
    
}
var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,

    ClientId = "client",
    ClientSecret = "secret",
    Scope = "api1"
});

if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    return;
}
//输出对应的token
Console.WriteLine(tokenResponse.Json);
 var apiClient = new HttpClient();
 //赋值token
apiClient.SetBearerToken(tokenResponse.AccessToken);
//调用接口
var response = await apiClient.GetAsync("https://localhost:6001/identity");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
    Console.ReadKey();
}
else
{
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine(JArray.Parse(content));
    Console.ReadKey();
}
```

