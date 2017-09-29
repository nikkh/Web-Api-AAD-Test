using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(AADApiTest_ApI.Startup))]
namespace AADApiTest_ApI
{
   

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
