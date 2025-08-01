using System.Collections.ObjectModel;
using System.Windows.Input;
using LocationShareApp.Services;
using CommunityToolkit.Mvvm.Input;

namespace LocationShareApp.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private string _bindingCode = string.Empty;
        private string _nickName = string.Empty;
        private string _errorMessage = string.Empty;
        private ObservableCollection<ConnectedUser> _connectedUsers;

        public UserManagementViewModel(IApiService apiService)
        {
            _apiService = apiService;
            _connectedUsers = new ObservableCollection<ConnectedUser>();
            
            Title = "用户管理";
            AddUserCommand = new AsyncRelayCommand(AddUserAsync, CanAddUser);
            RemoveUserCommand = new AsyncRelayCommand<ConnectedUser>(RemoveUserAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        }

        public string BindingCode
        {
            get => _bindingCode;
            set
            {
                SetProperty(ref _bindingCode, value);
                ((AsyncRelayCommand)AddUserCommand).NotifyCanExecuteChanged();
            }
        }

        public string NickName
        {
            get => _nickName;
            set
            {
                SetProperty(ref _nickName, value);
                ((AsyncRelayCommand)AddUserCommand).NotifyCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<ConnectedUser> ConnectedUsers
        {
            get => _connectedUsers;
            set => SetProperty(ref _connectedUsers, value);
        }

        public ICommand AddUserCommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand RefreshCommand { get; }

        public async Task InitializeAsync()
        {
            await LoadConnectedUsersAsync();
        }

        private bool CanAddUser()
        {
            return !string.IsNullOrWhiteSpace(BindingCode) && 
                   !string.IsNullOrWhiteSpace(NickName) && 
                   !IsBusy;
        }

        private async Task AddUserAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.AddConnectionAsync(BindingCode, NickName);
                
                if (result.IsSuccess)
                {
                    // 清空输入框
                    BindingCode = string.Empty;
                    NickName = string.Empty;
                    
                    // 刷新用户列表
                    await LoadConnectedUsersAsync();
                    
                    await Application.Current!.MainPage!.DisplayAlert("成功", "用户添加成功", "确定");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "添加用户失败";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"添加用户失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RemoveUserAsync(ConnectedUser? user)
        {
            if (user == null) return;

            var confirm = await Application.Current!.MainPage!.DisplayAlert(
                "确认删除", 
                $"确定要移除用户 {user.NickName} 吗？", 
                "确定", 
                "取消");

            if (!confirm) return;

            try
            {
                IsBusy = true;

                var result = await _apiService.RemoveConnectionAsync(user.Id);
                
                if (result.IsSuccess)
                {
                    ConnectedUsers.Remove(user);
                    await Application.Current!.MainPage!.DisplayAlert("成功", "用户移除成功", "确定");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert("失败", result.ErrorMessage ?? "移除用户失败", "确定");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("错误", $"移除用户失败: {ex.Message}", "确定");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshAsync()
        {
            await LoadConnectedUsersAsync();
        }

        private async Task LoadConnectedUsersAsync()
        {
            try
            {
                IsBusy = true;

                var result = await _apiService.GetConnectedUsersAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    ConnectedUsers.Clear();
                    foreach (var user in result.Data)
                    {
                        ConnectedUsers.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载关联用户失败: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}