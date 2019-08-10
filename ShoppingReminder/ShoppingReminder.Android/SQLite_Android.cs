using System;
using System.IO;
using Xamarin.Forms;
using ShoppingReminder.Repository;
using ShoppingReminder.Droid;

[assembly: Dependency(typeof(SQLite_Android)) ]
namespace ShoppingReminder.Droid
{
    class SQLite_Android : ISQLite
    {
        public SQLite_Android()
        {

        }
        public string GetDatabasePath(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, filename);
            return path;
        }
    }
}