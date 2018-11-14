namespace BangazonWorkforce.IntegrationTests
{
    public static class Config
    {
        public static string ConnectionSring
        {
            get
            {
                return "server=DESKTOP-LVBN4M3\\SQLEXPRESS;Initial Catalog=Bangazon;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            }
        }
    }
}
