using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FileSearch.ViewModels
{
   public class MainWindowViewModel
    {
       private ICollection<string> colaction = new List<string>();

        public  ICollection<string> Colaction => colaction;

        public MainWindowViewModel()
        {
             Thread thread = new Thread(GetItemsSearching) { IsBackground = true };
            thread.Start();
        }

        private void Search()
        {


        }



        public void GetItemsSearching()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                colaction.Add("fdgd");
                colaction.Add("fgfh");
                colaction.Add("67i7j");
                colaction.Add("vbnc");
                colaction.Add("zxgvdf");
                colaction.Add("fdhbfdgh");
                colaction.Add("gfhndfgt");
            });
        }




    }
}