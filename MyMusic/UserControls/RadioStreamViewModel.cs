﻿using MyMusic.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using GalaSoft.MvvmLight;

namespace MyMusic.ViewModels
{
    public class RadioStreamViewModel : INotifyPropertyChanged
    {
        #region properties

        private int _radioId;
        public int RadioId
        {
            get
            {
                return _radioId;
            }
            set
            {
                if (_radioId != value)
                {
                    _radioId = value;
                    NotifyPropertyChanged("RadioId");
                }
            }
        }

        private string _Name;
        public string RadioName
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    NotifyPropertyChanged("RadioName");
                }
            }
        }

        private string _genre;
        public string RadioGenre
        {
            get
            {
                return _genre;
            }
            set
            {
                if (_genre != value)
                {
                    _genre = value;
                    NotifyPropertyChanged("RadioGenre");
                }
            }
        }

        private string _image = "ms-appx:///Assets/music3.jpg";
        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        private string _Url;
        public string RadioUrl
        {
            get
            {
                return _Url;
            }
            set
            {
                if (_Url != value)
                {
                    _Url = value;
                    NotifyPropertyChanged("RadioUrl");
                }
            }
        }

        private string _type;
        public string RadioType
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    NotifyPropertyChanged("RadioType");
                }
            }
        }

        private int _failedAttempts;
        public int FailedAttempts
        {
            get
            {
                return _failedAttempts;
            }
            set
            {
                if (_failedAttempts != value)
                {
                    _failedAttempts = value;
                    NotifyPropertyChanged("FailedAttempts");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    public class RadioGenreViewModel : INotifyPropertyChanged
    {
        private int _radioGenreId;
        public int RadioGenreId
        {
            get
            {
                return _radioGenreId;
            }
            set
            {
                if (_radioGenreId != value)
                {
                    _radioGenreId = value;
                    NotifyPropertyChanged("RadioGenreId");
                }
            }
        }

        private int _radioGenreKey;
        public int RadioGenreKey
        {
            get
            {
                return _radioGenreKey;
            }
            set
            {
                if (_radioGenreKey != value)
                {
                    _radioGenreKey = value;
                    NotifyPropertyChanged("RadioGenreKey");
                }
            }
        }

        private string _radioGenreName;
        public string RadioGenreName
        {
            get
            {
                return _radioGenreName;
            }
            set
            {
                if (_radioGenreName != value)
                {
                    _radioGenreName = value;
                    NotifyPropertyChanged("RadioGenreName");
                }
            }
        }

        private string _image = "ms-appx:///Assets/music3.jpg";
        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        private int _group;
        public int Group
        {
            get
            {
                return _group;
            }
            set
            {
                if (_group != value)
                {
                    _group = value;
                    NotifyPropertyChanged("Group");
                }
            }
        }

        private int _sectionNo;
        public int SectionNo
        {
            get
            {
                return _sectionNo;
            }
            set
            {
                if (_sectionNo != value)
                {
                    _sectionNo = value;
                    NotifyPropertyChanged("SectionNo");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }



    public class RadioStreamsViewModel : ViewModelBase
    {
        private HttpClient client = new HttpClient();
       
       
        private ObservableCollection<RadioStreamViewModel> _radioStreams;
        public ObservableCollection<RadioStreamViewModel> RadioStreams
        {
            get
            {
                return _radioStreams;
            }

            set
            {
                _radioStreams = value;
                RaisePropertyChanged("RadioStreams");
            }
        }

        private ObservableCollection<RadioGenreViewModel> _radioGenres;
        public ObservableCollection<RadioGenreViewModel> RadioGenres
        {
            get
            {
                return _radioGenres;
            }

            set
            {
                _radioGenres = value;
                RaisePropertyChanged("RadioGenres");
            }
        }

        public ObservableCollection<RadioGenreViewModel> GetRadioGenres()
        {            
            _radioGenres = new ObservableCollection<RadioGenreViewModel>();
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var rdos = db.Table<RadioGenre>().ToList();
                //SortGenreGroups(rdos);
                //var rdoss = db.Table<RadioGenre>().ToList();
                foreach (var tr in rdos)
                {
                    var rdo = new RadioGenreViewModel()
                    {
                        RadioGenreId = tr.RadioGenreId,
                        RadioGenreName = tr.RadioGenreName,
                        RadioGenreKey = Convert.ToInt32(tr.RadioGenreKey),
                        Image = tr.RadioImage,
                        Group = tr.Group,
                        SectionNo = tr.SectionNo
                    };
                    _radioGenres.Add(rdo);
                }
            }
            return _radioGenres;
        }

        public ObservableCollection<RadioStreamViewModel> GetRadioStationsXml(int genreId)
        {
            _radioStreams = new ObservableCollection<RadioStreamViewModel>();
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var gns = db.Table<RadioGenre>().ToList();
                var rdos = db.Table<RadioStream>().Where(a => a.RadioGenreId == genreId).ToList();
                var fg = db.Table<RadioStream>().ToList();
                foreach (var tr in rdos)
                {
                    string name = tr.RadioName;
                    if(string.IsNullOrEmpty(name))
                    {
                        name = (tr.RadioUrl).Split('/')[((tr.RadioUrl).Split('/').Count()) - 1];
                    }
                    string img = gns.Where(x => x.RadioGenreKey == genreId.ToString()).Select(a => a.RadioImage).FirstOrDefault();
                    var rdo = new RadioStreamViewModel()
                    {
                        RadioId = tr.RadioId,
                        RadioName = name,                       
                        RadioUrl = tr.RadioUrl,
                        Image = img
                    };
                    _radioStreams.Add(rdo);
                }
            }
            return _radioStreams;
        }

        public ObservableCollection<RadioStreamViewModel> GetRadioStations()
        {
            _radioStreams = new ObservableCollection<RadioStreamViewModel>();
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var sts = db.Table<RadioStream>().ToList();
                foreach (var tr in sts)
                {
                    var rdo = new RadioStreamViewModel()
                    {
                        RadioId = tr.RadioId,
                        RadioName = tr.RadioName,
                        RadioUrl = tr.RadioUrl,
                        Image = tr.Image,
                        RadioGenre = tr.RadioGenreId.ToString()
                    };
                    _radioStreams.Add(rdo);
                }
            }
            return _radioStreams;
        }

        public ObservableCollection<RadioStreamViewModel> GetRadioStationsTest(int genreId)
        {
            _radioStreams = new ObservableCollection<RadioStreamViewModel>();
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var gns = db.Table<RadioGenre>().ToList();
                var rdos = db.Table<RadioStream>().Where(a => a.RadioGenreId == genreId && a.RadioType == "Dirble").ToList();
                var fg = db.Table<RadioStream>().ToList();
                foreach (var tr in rdos)
                {
                    string name = tr.RadioName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = (tr.RadioUrl).Split('/')[((tr.RadioUrl).Split('/').Count()) - 1];
                    }
                    string img = gns.Where(x => x.RadioGenreKey == genreId.ToString()).Select(a => a.RadioImage).FirstOrDefault();
                    var rdo = new RadioStreamViewModel()
                    {
                        RadioId = tr.RadioId,
                        RadioName = name,
                        RadioUrl = tr.RadioUrl,
                        RadioType = tr.RadioType,
                        Image = img
                    };
                    _radioStreams.Add(rdo);
                }
            }
            return _radioStreams;
        }

        public void AddFailedAttempt(string rNme)
        {
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {
                var st = db.Table<RadioStream>().Where(a => a.RadioName == rNme).FirstOrDefault();
                if (st != null)
                {
                    st.FailedAttempts++;
                    db.Update(st);
                }
            }
        }

        //public async Task GetTest()
        //{
        //    _radioGenres = new ObservableCollection<RadioGenreViewModel>();
        //    client.BaseAddress = new Uri("http://api.dirble.com/v1/");
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    string resp = await client.GetStringAsync("stations/apikey/2525c6b82d53ec0a6f9b58ea5efc7d27aa1109ea/id/11");
        //    var result = JsonConvert.DeserializeObject<List<MyMusic.test.RootObject>>(resp);
        //    List<RadioStream> stationList = new List<RadioStream>();
        //    foreach (var m in result)
        //    {
        //        RadioStream st = new RadioStream {  RadioName = m.name, RadioUrl = m.streamurl };
        //        stationList.Add(st);
        //    }
        //}

        

        private void SortGenreGroups(List<RadioGenre> gns)
        {
            int div = 1;
            int counter = 1;
            using (var db = new SQLite.SQLiteConnection(App.DBPath))
            {              
                foreach (var item in gns)
                {

                    var st = db.Table<RadioGenre>().Where(a => a.RadioGenreName == item.RadioGenreName).FirstOrDefault();
                    st.Group = div;
                    st.SectionNo = counter;
                    db.Update(st);

                    counter++;
                    if (counter == 6) { div++; counter = 1; }
                    
                }                               
            }
            
        }   
           
        public void AddGenrePics()
        {        
            using (var db = new SQLite.SQLiteConnection(App.DBPath))    //Data/Stations.xml
            {
         
                int cnt = db.Table<RadioGenre>().Count();
                try
                {
                    var rdos = db.Table<RadioGenre>().ToList();
                    foreach (var tr in rdos)
                    {
                        if((tr.RadioGenreName.ToLower()).Contains("rock and roll"))
                        { tr.RadioImage = "ms-appx:///Assets/floyd.jpg"; }

                        else if ((tr.RadioGenreName.ToLower()).Contains("alternative"))
                        { tr.RadioImage = "ms-appx:///Assets/rock.jpg"; }

                        else if ((tr.RadioGenreName.ToLower()).Contains("jazz"))
                        { tr.RadioImage = "ms-appx:///Assets/jazz2.jpg"; }

                        else if ((tr.RadioGenreName.ToLower()).Contains("country"))
                        { tr.RadioImage = "ms-appx:///Assets/country2.jpg"; }

                        else if ((tr.RadioGenreName.ToLower()).Contains("dance"))
                        { tr.RadioImage = "ms-appx:///Assets/dance.jpg"; }

                        db.Update(tr);
                    }
                }
                catch (Exception ex)
                {
                    string g = ex.InnerException.Message;
                }
                cnt = db.Table<RadioStream>().Count();
            }
        }

    }
    public class RootObject
    {
        public int Id { get; set; }
        public string RadioName { get; set; }
        public string RadioUrl { get; set; }
        public object Image { get; set; }
        public string StatnType { get; set; }
        public string RadioGenreName { get; set; }
        public int? RadioGenreId { get; set; }
        public int Status { get; set; }
        public object RadioGenre { get; set; }
    }
}



//public async Task<List<RadioGenre>> LastFm()
//{
//    filter = new HttpBaseProtocolFilter();
//    httpClient = new HttpClient(filter);
//    cts = new CancellationTokenSource();
//    List<RadioGenre> geners = new List<RadioGenre>();

//    Uri resourceUri;
//    string queryString = string.Format("http://streamfinder.com/api/index.php?api_codekey={0}&return_data_format=xml&do=get_genre_list", key);

//    if (!Helpers.TryGetUri(queryString, out resourceUri))
//    {
//        return null;
//    }
//    try
//    {
//        HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
//        //var response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
//        var xmlString = response.Content.ReadAsStringAsync().GetResults();

//        XDocument doc = XDocument.Parse(xmlString);

//        List<XElement> genList = (from a in doc.Descendants("genre")
//                                  select a).ToList();
//        if (genList != null)
//        {
//            foreach (XElement item in genList)
//            {
//                var gid = item.Element("gid").Value;
//                var _name = item.Element("genre_name").Value;
//                geners.Add(new RadioGenre { RadioGenreName = _name.ToString(), RadioGenreKey = gid.ToString() });
//            }
//        }
//    }
//    catch (Exception exx) { string error = exx.Message; }

//    return geners;
//}

//public async Task<List<RadioGenre>> Getgenres()
//{
//    filter = new HttpBaseProtocolFilter();
//    httpClient = new HttpClient(filter);
//    cts = new CancellationTokenSource();
//    List<RadioGenre> geners = new List<RadioGenre>();

//    Uri resourceUri;
//    string queryString = string.Format("http://streamfinder.com/api/index.php?api_codekey={0}&return_data_format=xml&do=get_genre_list", key);

//    if (!Helpers.TryGetUri(queryString, out resourceUri))
//    {
//        return null;
//    }
//    try
//    {
//        HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
//        //var response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
//        var xmlString = response.Content.ReadAsStringAsync().GetResults();

//        XDocument doc = XDocument.Parse(xmlString);

//        List<XElement> genList = (from a in doc.Descendants("genre")
//                                  select a).ToList();
//        if (genList != null)
//        {
//            foreach (XElement item in genList)
//            {
//                var gid = item.Element("gid").Value;
//                var _name = item.Element("genre_name").Value;
//                geners.Add(new RadioGenre { RadioGenreName = _name.ToString(), RadioGenreKey = gid.ToString(), RadioImage = "ms-appx:///Assets/music3.jpg" });
//            }
//        }
//    }
//    catch (Exception exx) { string error = exx.Message; }

//    return geners;
//}

//public async Task<IEnumerable<RadioStream>> GetRadioStations(string gid)
//{
//    filter = new HttpBaseProtocolFilter();
//    httpClient = new HttpClient(filter);
//    cts = new CancellationTokenSource();
//    List<RadioStream> raders = new List<RadioStream>();

//    Uri resourceUri;
//    string queryString = string.Format("http://streamfinder.com/api/index.php?api_codekey={0}&return_data_format=xml&do=genre_search&gid={1}&format=mp3", key, gid);

//    if (!Helpers.TryGetUri(queryString, out resourceUri))
//    {
//        return null;
//    }
//    try
//    {
//        HttpResponseMessage response = await httpClient.GetAsync(resourceUri).AsTask(cts.Token);
//        var xmlString = response.Content.ReadAsStringAsync().GetResults();

//        XDocument doc = XDocument.Parse(xmlString);
//        List<XElement> rdoList = (from a in doc.Descendants("data")
//                                  select a).ToList();
//        if (rdoList != null)
//        {
//            foreach (XElement data in rdoList)
//            {
//                var _name = data.Element("name").Value;
//                var url = (from a in data.Element("streams").Descendants("stream_url")
//                           select a).First().Value;
//                if ((from a in raders where a.RadioName == _name || a.RadioUrl == url select a).Count() < 1)
//                {
//                    raders.Add(new RadioStream { RadioName = _name.ToString(), RadioUrl = url.ToString() });
//                }
//            }
//        }
//    }
//    catch (Exception exx) { string error = exx.Message; }

//    return raders;
//}


//public async void AddRadios()
//{
//    //await readXMLAsync();
//    Stations sts = new Stations();
//    using (var db = new SQLite.SQLiteConnection(App.DBPath))    //Data/Stations.xml
//    {
//        int cnt = db.Table<RadioStream>().Count();
//        int count = db.Table<RadioGenre>().Count();
//        db.DeleteAll<RadioStream>();
//        db.DeleteAll<RadioGenre>();
//        cnt = db.Table<RadioStream>().Count();
//        count = db.Table<RadioGenre>().Count();
//        try
//        {
//            XmlSerializer xs = new XmlSerializer(typeof(Stations), new Type[] { typeof(radioStation) });
//            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/Stations.xml"));
//            using (var fileStream = await file.OpenStreamForReadAsync())
//            {
//                sts = (Stations)xs.Deserialize(fileStream);
//            }

//            IEnumerable<string> strs = sts.stations.Select(a => a.Genre).Distinct();

//            foreach (var item in strs)
//            {
//                RadioGenre rds = new RadioGenre
//                {
//                    RadioGenreName = item,
//                    RadioImage = "ms-appx:///Assets/music3.jpg"
//                };
//                db.Insert(rds);
//            }

//            foreach (radioStation item in sts.stations.ToList())
//            {
//                RadioGenre r = db.Table<RadioGenre>().Where(a => a.RadioGenreName == item.Genre).FirstOrDefault();
//                RadioStream rds = new RadioStream
//                {
//                    RadioName = item.Name,
//                    RadioUrl = item.Urls.FirstOrDefault().urlName,
//                    RadioGenreId = r.RadioGenreId
//                };
//                db.Insert(rds);
//            }

//        }
//        catch (Exception ex)
//        {
//            string g = ex.InnerException.Message;
//        }
//        cnt = db.Table<RadioStream>().Count();
//        count = db.Table<RadioGenre>().Count();
//    }
//}

//public async void AddGenre()
//{
//    //await readXMLAsync();
//    Stations sts = new Stations();
//    using (var db = new SQLite.SQLiteConnection(App.DBPath))    //Data/Stations.xml
//    {
//        db.DropTable<RadioGenre>();
//        int cnt = db.Table<RadioGenre>().Count();
//        try
//        {
//            XmlSerializer xs = new XmlSerializer(typeof(Stations), new Type[] { typeof(radioStation) });
//            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/Stations.xml"));
//            using (var fileStream = await file.OpenStreamForReadAsync())
//            {
//                sts = (Stations)xs.Deserialize(fileStream);
//            }

//            IEnumerable<string> strs = sts.stations.Select(a => a.Genre).Distinct();

//            foreach (var item in strs)
//            {
//                RadioGenre rds = new RadioGenre
//                {
//                    RadioGenreName = item,
//                    RadioImage = "ms-appx:///Assets/music3.jpg"
//                };
//                db.Insert(rds);
//            }
//        }
//        catch (Exception ex)
//        {
//            string g = ex.InnerException.Message;
//        }
//        cnt = db.Table<RadioStream>().Count();
//    }
//}

