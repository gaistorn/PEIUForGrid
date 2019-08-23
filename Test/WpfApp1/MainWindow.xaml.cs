using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Flurl;
using Flurl.Http;
using PEIU.UI.Model;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
             loadFlurl();
            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("https://www.peiu.co.kr:3021/");

            //// JSON 형식에 대한 Accept 헤더를 추가합니다.
            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));

            //// 모든 제품들의 목록.
            //HttpResponseMessage response = client.GetAsync("api/contract/getcontractorlist").Result;  // 호출 블록킹!
            //if (response.IsSuccessStatusCode)
            //{
            //    // 응답 본문 파싱. 블록킹!
            //    var products = response.Content.ReadAsStringAsync();
            //    Console.WriteLine(products);
            //    //foreach (var p in products)
            //    //{
            //    //    Console.WriteLine("{0}\t{1};\t{2}", p.Name, p.Price, p.Category);
            //    //}
            //}
            //else
            //{
            //    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            //}
        }

        private async void loadFlurl()
        {
            //// Flurl will use 1 HttpClient instance per host
            //var person = await "https://www.peiu.co.kr:3021"
            //    .AppendPathSegment("api/contract/getcontractorlist")
            //    .GetStringAsync();
            //Console.WriteLine(person);

            // Flurl will use 1 HttpClient instance per host
            var person = await "https://www.peiu.co.kr:3021"
                .AppendPathSegment("api/auth/login")
                .PostJsonAsync(new
                {
                    Email = "redwinelove@hotmail.com",
                    Password = "Kkr753951!",
                    RememberMe = "False"
                })
                .ReceiveJson<SignInResultModel>();
            Console.WriteLine(person.Result.Succeeded);

            if(person.Result.Succeeded)
            {
                var login_test = await "https://www.peiu.co.kr:3021"
                .AppendPathSegment("api/auth/me")
                .WithOAuthBearerToken(person.JWToken)
                .GetStringAsync();
                Console.WriteLine(login_test);
            }
        }

//        {
//	"Email" : "redwinelove@hotmail.com",
//	"Password" : "Kkr753951!",
//	"RememberMe" : "False"
//}
}
}
