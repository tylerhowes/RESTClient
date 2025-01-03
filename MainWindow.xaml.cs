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
    //MAIN WINDOW OF APPLICATION
    public partial class MainWindow : Window
    {
        private string _userID;
        HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            ReadRoomDataAsync();
        }


        private string GetUserID()
        {
            if (string.IsNullOrEmpty(_userID))
            {
                EnterUserID userIDTextBox = new EnterUserID();
                bool? dialogResult = userIDTextBox.ShowDialog();

                if (dialogResult == true)
                {
                    _userID = userIDTextBox.UserID;
                }
                
            }

            return _userID;
        }

        public void ChangeUser(object sender, RoutedEventArgs e)
        {
            EnterUserID userIDTextBox = new EnterUserID();
            bool? dialogResult = userIDTextBox.ShowDialog();

            if (dialogResult == true)
            {
                _userID = userIDTextBox.UserID;
            }
        }

        //ROOM RELATED METHODS  ===================================================================================================================================
        //Async room data method presents the rooms in a List of RoomData objects which is used as item source in XAML
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

        //Uses the orchestrator get request to access rooms in the data base
        static async Task<string> GetRoomsAsync(HttpClient httpClient)
        {

            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/roomDB");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        //Method to register the user has clicked the apply button
        private void AppliedClicked(object sender, RoutedEventArgs e)
        {
            RoomData.RoomData roomItem = RoomDataGrid.SelectedItem as RoomData.RoomData;
            Task<bool> task = postApplication(roomItem, client);
        }

        //Method that is async while client communicates with orchestrator after room is applied for, returns value dependant on HTTP Response
        //Room data is serialized, similar to gson but uses newtonsoft library. And then uses post request in orchestrator
        private async Task<bool> postApplication(RoomData.RoomData roomData, HttpClient client)
        {           
            string userID = GetUserID();
            StringContent roomDataJSON = new StringContent(JsonConvert.SerializeObject(roomData), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("http://localhost:8080/RESTService/webresources/userApplication?UserID=" + userID, roomDataJSON);

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
        //APPLICATION AND HISTORY RELATED METHODS  ==================================================================================================================

        //UI Button that allows the user to view existing applications. The orchestrator returns the appropriate data from the database in JSON format
        //Data in the UI DataGrid is set accordingly after deserialising JSON into List of ApplicationData objects
        public async void ViewApplicationsAsync(object sender, RoutedEventArgs e)
        {
            string userID = GetUserID();
            string json = await GetApplicationsAsync(client, userID);
            
            List<ApplicationData> roomList = JsonConvert.DeserializeObject<List<ApplicationData>>(json);

            ApplicationsDataGrid.ItemsSource = roomList;

            ApplicationsDataGrid.Columns[5].Visibility = Visibility.Visible;
            RoomDataGrid.Visibility = Visibility.Collapsed;
            ApplicationsDataGrid.Visibility = Visibility.Visible;
        }

        //Uses the orchestrator GET request to retrieve data on applications presented in JSON format
        static async Task<string> GetApplicationsAsync(HttpClient httpClient, string userID)
        {
            
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/userApplication/applications?UserID=" + userID);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Trace.WriteLine("THIS IS THE JSON FOR APPLICATIONS: " + jsonResponse);
            return jsonResponse;
        }

        //UI Button to return to the table that shows available rooms
        public void ViewRooms(object sender, RoutedEventArgs e)
        {
            ApplicationsDataGrid.Visibility = Visibility.Collapsed;
            RoomDataGrid.Visibility = Visibility.Visible;
        }

        //UI button method for if the user wishes to cancel an existing application, sends request to orchestrator as a PUT request
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
       
            Task<bool> task = putApplication(application, client);

        }

        //Uses the orchestrator PUT request to update an application with the cancelled status, passes the corresponding application json to the orchestrator for management
        private async Task<bool> putApplication(ApplicationData applicationData, HttpClient client)
        {
            StringContent applicationDataJSON = new StringContent(JsonConvert.SerializeObject(applicationData), Encoding.UTF8, "application/json");
            Trace.WriteLine("THIS IS THE JSON: " + await applicationDataJSON.ReadAsStringAsync());
            HttpResponseMessage response = await client.PutAsync("http://localhost:8080/RESTService/webresources/userApplication/cancel", applicationDataJSON);

            if (response.IsSuccessStatusCode)
            {
                //simulates a button click
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


        //Uses orchestrator GET request to return the list of room applications history
        static async Task<string> GetAppHistoryAsync(HttpClient httpClient, string userID)
        {
            
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/userApplication/history?UserID=" + userID);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return jsonResponse;
        }

        //UI Button for if the use wishes to view history of applications
        private async void ViewHistory(object sender, RoutedEventArgs e)
        {
            string userID = GetUserID();
            string json = await GetAppHistoryAsync(client, userID);

            List<ApplicationData> roomList = JsonConvert.DeserializeObject<List<ApplicationData>>(json);

            ApplicationsDataGrid.ItemsSource = roomList;

            ApplicationsDataGrid.Columns[5].Visibility = Visibility.Collapsed;
            RoomDataGrid.Visibility = Visibility.Collapsed;
            ApplicationsDataGrid.Visibility = Visibility.Visible;
        }

        //WEATHER RELATED METHODS  ===================================================================================================================================
        //Method to register if user clicked the check weather button in the UI, sends the 
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

        //Uses the orchestrator GET request to retrieve weather JSON  that is converted to object and used to construct weather string on client side
        static async Task<string> GetWeatherAsync(HttpClient httpClient, string postcode)
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/weather?postcode=" + postcode);
            response.EnsureSuccessStatusCode();

            //have to read the response in this way or data is incorrect format
            var responseContent = await response.Content.ReadAsStringAsync();
            //Trace.WriteLine("THIS IS RESPONSE: " + responseContent);
            WeatherObject weatherObj = JsonConvert.DeserializeObject<WeatherObject>(responseContent);

            string responseString = "Weather: " + weatherObj.weather + "\nWind Speed: " + weatherObj.windSpeed + "\nTemperature: " + weatherObj.temperature;

            return responseString;
        }


        //DISTANCE RELATED METHODS  ===================================================================================================================================
        //Check distance method for button click on UI, converts user entered postcode into acceptable format before trying to use API
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
        
        //Orchestrator GET request that returns JSON that is deserialised into object for storage and use in client
        static async Task<string> GetDistanceAsync(HttpClient httpClient, string postcode1, string postcode2)
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/location?postcode1=" + postcode1 + "&postcode2=" + postcode2);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            RouteObject routeObj = JsonConvert.DeserializeObject<RouteObject>(responseContent);
            Trace.WriteLine("DISTANCE: " + routeObj.distance.ToString());
            return routeObj.distance.ToString(); ;
        }


        //CRIME RELATED METHODS  ===================================================================================================================================
        //Check crime method for button click on UI, converts user entered postcode into acceptable format before trying to use API
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

        //Method for using the orchestrator to get crime data
        static async Task<string> GetCrimeAsync(HttpClient httpClient, string postcode)
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:8080/RESTService/webresources/crime?postcode=" + postcode);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            CrimeReportObject crimeReport = JsonConvert.DeserializeObject<CrimeReportObject>(responseContent);
            Trace.WriteLine("Current crime report: " + crimeReport.crimePairs);

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

       
        
    }
}
