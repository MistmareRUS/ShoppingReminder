using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingReminder.Repository
{
    public interface ISQLite
    {
        string GetDatabasePath(string filename);
    }
}
