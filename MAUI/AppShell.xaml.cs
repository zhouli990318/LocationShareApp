using LocationShareApp.Views;

namespace LocationShareApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// 注册路由
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("register", typeof(RegisterPage));
		Routing.RegisterRoute("userdetail", typeof(UserDetailPage));
		Routing.RegisterRoute("usermanagement", typeof(UserManagementPage));
	}
}
