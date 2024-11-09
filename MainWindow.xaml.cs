using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrimeReport;
using Newtonsoft.Json;

namespace RESTClientService
{
    public partial class MainWindow : Window
    {

        HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            ReadRoomDataAsync();
        }

        private async void ReadRoomDataAsync()
        {
           
            string json = await GetRoomsAsync(client);


            List<RoomData.RoomData> roomList = JsonConvert.DeserializeObject<List<RoomData.RoomData>>(json);
            if (roomList != null && roomList.Count > 0)
            {
                RoomDataGrid.ItemsSource = roomList; 
            }
            else
            {
                MessageBox.Show("No rooms found in JSON.");
            }
        }

        static async Task<string> GetRoomsAsync(HttpClient httpClient)
        {

            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/roomDB");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        private void UserApplied(object sender, RoutedEventArgs e)
        {
            RoomData.RoomData roomItem = RoomDataGrid.SelectedItem as RoomData.RoomData;
            Task<bool> task = postApplication(roomItem, client);
        }

        private async Task<bool> postApplication(RoomData.RoomData roomData, HttpClient client)
        {
            StringContent roomDataJSON = new StringContent(JsonConvert.SerializeObject(roomData), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://localhost:8080/RESTService/webresources/userApplication", roomDataJSON);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("You have successfully applied for this room");
                return true; 
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                MessageBox.Show("Room application is alread pending or accepted\nCheck Application History");
                return false;
            }
            else
            {                    
                string errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error adding application: " + errorMessage);
                return false;
            }
            
        }

        private async void CheckWeather(object sender, RoutedEventArgs e)
        {
            RoomData.RoomData roomItem = RoomDataGrid.SelectedItem as RoomData.RoomData;
            string roomPostcode = roomItem.location.postcode;
            roomPostcode = roomPostcode.Replace(" ", "");
            string weatherString = "";
            try
            {
                LoadingIcon.Visibility = Visibility.Visible;                             
                weatherString = await GetWeatherAsync(client, roomPostcode);                          
            }
            finally
            {           
                LoadingIcon.Visibility = Visibility.Collapsed;
                MessageBox.Show(weatherString);               
            }            
        }

        static async Task<string> GetWeatherAsync(HttpClient httpClient, string postcode)
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/weather?postcode=" + postcode);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            Trace.WriteLine("THIS IS RESPONSE: " + responseContent);
            WeatherObject weatherObj = JsonConvert.DeserializeObject<WeatherObject>(responseContent);

            string responseString = "Weather: " + weatherObj.weather + "\nWind Speed: " + weatherObj.windSpeed + "\nTemperature: " + weatherObj.temperature;

            return responseString;
        }

        private async void CheckDistance(object sender, RoutedEventArgs e)
        {

            RoomData.RoomData roomItem = RoomDataGrid.SelectedItem as RoomData.RoomData;
            string roomPostcode = roomItem.location.postcode;
            string userPostcode = postcodeText.Text;

            userPostcode = userPostcode.Replace(" ", "");
            roomPostcode = roomPostcode.Replace(" ", "");

            string distanceString = "";
            
            try
            {              
                LoadingIcon.Visibility = Visibility.Visible;                             
                distanceString = await GetDistanceAsync(client, roomPostcode, userPostcode);
            }
            finally
            {
                LoadingIcon.Visibility = Visibility.Collapsed;
                MessageBox.Show("Distance to address: " + distanceString);
            }   
        }
        
        static async Task<string> GetDistanceAsync(HttpClient httpClient, string postcode1, string postcode2)
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/location?postcode1=" + postcode1 + "&postcode2=" + postcode2);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            RouteObject routeObj = JsonConvert.DeserializeObject<RouteObject>(responseContent);
            Trace.WriteLine("DISTANCE: " + routeObj.distance.ToString());
            return routeObj.distance.ToString(); ;
        }

        
        private async void CheckCrime(object sender, RoutedEventArgs e)
        {
            RoomData.RoomData roomItem = RoomDataGrid.SelectedItem as RoomData.RoomData;
            string roomPostcode = roomItem.location.postcode;
            roomPostcode = roomPostcode.Replace(" ", "");
            string crimeString = "";
            try
            {
                LoadingIcon.Visibility = Visibility.Visible;            
                crimeString = await GetCrimeAsync(client, roomPostcode);

            } finally
            {         
                LoadingIcon.Visibility = Visibility.Collapsed;
                MessageBox.Show(crimeString);
            }  
        }

        static async Task<string> GetCrimeAsync(HttpClient httpClient, string postcode)
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/crime?postcode=" + postcode);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            CrimeReportObject crimeReport = JsonConvert.DeserializeObject<CrimeReportObject>(responseContent);
            Trace.WriteLine("Current crime report: " + crimeReport.crimePairs);
            //Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();

            //foreach (CrimeReportObject crimeReport in crimeReportList)
            //{               
            //    if (keyValuePairs.ContainsKey(crimeReport.Category))
            //    {
            //        keyValuePairs[crimeReport.Category] += 1;
            //    }
            //    else
            //    {
            //        keyValuePairs.Add(crimeReport.Category, 1);
            //    }
            //}

            string crimeLogString = "";
            foreach (var entry in crimeReport.crimePairs)
            {
                crimeLogString += entry.Key + ": " + entry.Value + "\n";
            }

            if (crimeLogString == "")
            {
                crimeLogString = "There is no crime to report";
            }

            return crimeLogString;
        }

        public async void ViewApplicationsAsync(object sender, RoutedEventArgs e)
        {
            string json = await GetApplicationsAsync(client);

            List<ApplicationData> roomList = JsonConvert.DeserializeObject<List<ApplicationData>>(json);

            ApplicationsDataGrid.ItemsSource = roomList;

            ApplicationsDataGrid.Columns[4].Visibility = Visibility.Visible;
            RoomDataGrid.Visibility = Visibility.Collapsed;
            ApplicationsDataGrid.Visibility = Visibility.Visible;           
        }

        public void ViewRooms(object sender, RoutedEventArgs e)
        {

            ApplicationsDataGrid.Visibility = Visibility.Collapsed;
            RoomDataGrid.Visibility = Visibility.Visible;
        }

        public void UserCancelled(object sender, RoutedEventArgs e)
        {
            ApplicationData application = new ApplicationData();
            for (int i = 0; i < ApplicationsDataGrid.Items.Count; i++)
            {
                var item = ApplicationsDataGrid.Items[i];
                application.applicationID = (item as ApplicationData).applicationID;
                application.roomID = (item as ApplicationData).roomID;
                application.name = (item as ApplicationData).name;
                application.status = (item as ApplicationData).status;

            }
            //string applicationDataJson = JsonConvert.SerializeObject(application);
            Task<bool> task = putApplication(application, client);
            //MessageBox.Show(applicationDataJson);
        }


        private async Task<bool> putApplication(ApplicationData applicationData, HttpClient client)
        {
            StringContent applicationDataJSON = new StringContent(JsonConvert.SerializeObject(applicationData), Encoding.UTF8, "application/json");
            Trace.WriteLine("THIS IS THE JSON: " + await applicationDataJSON.ReadAsStringAsync());
            HttpResponseMessage response = await client.PutAsync("http://localhost:8080/RESTService/webresources/userApplication/cancel", applicationDataJSON);

            if (response.IsSuccessStatusCode)
            {
                ViewApplicationButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                MessageBox.Show("This application has been cancelled");
                return true;
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error updating application: " + errorMessage);
                return false;
            }

        }


        static async Task<string> GetApplicationsAsync(HttpClient httpClient)
        {

            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/userApplication/applications");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return jsonResponse;
        }

        static async Task<string> GetAppHistoryAsync(HttpClient httpClient)
        {

            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/userApplication/history");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return jsonResponse;
        }

        private async void ViewHistory(object sender, RoutedEventArgs e)
        {
            string json = await GetAppHistoryAsync(client);

            List<ApplicationData> roomList = JsonConvert.DeserializeObject<List<ApplicationData>>(json);

            ApplicationsDataGrid.ItemsSource = roomList;

            ApplicationsDataGrid.Columns[4].Visibility = Visibility.Collapsed;
            RoomDataGrid.Visibility = Visibility.Collapsed;
            ApplicationsDataGrid.Visibility = Visibility.Visible;
        }
    }
}
