using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PEIU.GUI.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public class SigninViewModel : ViewModelBase
    {
        private string _email;
        public string Email { get => _email; set => this.Set("Email", ref _email, value); }

        private string _password;
        public string Password { get => _password; set => this.Set("Password", ref _password, value); }

        public bool IsRemember
        {
            get => Settings.Default.LastSignRemember;
            set
            {
                Settings.Default.LastSignRemember = value;
                base.RaisePropertyChanged("IsRemember");
            }
        }

        public SigninViewModel()
        {
            if(IsRemember)
            {
                Email = Settings.Default.LastSignInEmail;
                Password = Settings.Default.LastSignInPassword;
            }
        }

        private RelayCommand _signInCommand;

        public RelayCommand SigninCommand { get => _signInCommand ?? (_signInCommand = new RelayCommand(SignIn)); }

        private void SignIn()
        {

        }

    }
}
