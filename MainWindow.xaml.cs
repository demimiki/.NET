using Newtonsoft.Json;
using OrvosKliens.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace OrvosKliens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Boolean start = true;
        public MainWindow()
        {
            InitializeComponent();
            listView.ItemsSource = null;
            GetPeople();
            start = false;
        }

        public void GetPeople()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var result = httpClient.GetAsync("http://localhost:80/person").Result;

                    string json = result.Content.ReadAsStringAsync().Result;
                    var people = JsonConvert.DeserializeObject<IEnumerable<Person>>(json);

                    listView.ItemsSource = people;
                }
            }
            catch (System.AggregateException)
            {
                if (start == true)
                {
                    MessageBox.Show("Nincs kapcsolat, indítás sikertelen.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }
                else
                {
                    MessageBox.Show("Nincs kapcsolat!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    listView.ItemsSource = null;
                    return;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeletePerson();
            nevTextBox.Text = "";
            cimTextBox.Text = "";
            tajszamTextBox.Text = "";
            panaszTextBox.Text = "";
            idoTextBox.Text = "";
        }

        private void DeletePerson()
        {
            if (listView.SelectedIndex != -1)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        string output = "http://localhost:80/person/" + "?id=" + listView.SelectedIndex;
                        var result = httpClient.DeleteAsync(output).Result;
                        if (!result.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Hiba törlés közben!","Hiba!",MessageBoxButton.OK,MessageBoxImage.Information);
                            return;
                        }
                    }
                }
                catch (System.AggregateException)
                {
                    MessageBox.Show("Nincs kapcsolat!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    listView.ItemsSource = null;
                    return;
                }
            }
            GetPeople();
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            DeletePerson();
            var nev = nevTextBox.Text;
            var cim = cimTextBox.Text;
            var tajszam = tajszamTextBox.Text;
            var panasz = panaszTextBox.Text;
            var idopont = DateTime.Now;

            nevTextBox.Text = "";
            cimTextBox.Text = "";
            tajszamTextBox.Text = "";
            panaszTextBox.Text = "";
            idoTextBox.Text = "";

            if (string.IsNullOrEmpty(nev) || string.IsNullOrEmpty(cim) || string.IsNullOrEmpty(tajszam))
            {
                MessageBox.Show("Invalid data!");
                return;
            }

            var person = new Person { Nev = nev, Lakcim = cim,  Tajszam= tajszam, Panasz=panasz, Idopont=idopont};

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(person);
                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var result = httpClient.PostAsync("http://localhost:80/person", stringContent).Result;
                }
            }
            catch (System.AggregateException)
            {
                MessageBox.Show("Nincs kapcsolat!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                listView.ItemsSource = null;
                return;
            }
            GetPeople();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedIndex != -1)
            {
                var selected = listView.SelectedIndex;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        string output = "http://localhost:80/person/";
                        var result = httpClient.GetAsync(output).Result;

                        string json = result.Content.ReadAsStringAsync().Result;
                        var people = JsonConvert.DeserializeObject<IEnumerable<Person>>(json);

                        Person[] j = people.ToArray<Person>();
                        nevTextBox.Text = j[selected].Nev;
                        cimTextBox.Text = j[selected].Lakcim;
                        tajszamTextBox.Text = j[selected].Tajszam;
                        panaszTextBox.Text = j[selected].Panasz;
                        idoTextBox.Text = ToStringDate(j[selected].Idopont);
                    }
                }
                catch (System.AggregateException)
                {
                    if (start == true)
                    {
                        MessageBox.Show("Nincs kapcsolat, indítás sikertelen.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        MessageBox.Show("Nincs kapcsolat!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        listView.ItemsSource = null;
                        return;
                    }
                }
            }
        }

        public string ToStringDate(DateTime dt)
        {
            return $"{dt:yy.MM.dd. HH:mm:ss}";
        }
    }
}
