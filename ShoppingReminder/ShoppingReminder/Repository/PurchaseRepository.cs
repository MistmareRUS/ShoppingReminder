using ShoppingReminder.Model;
using SQLite;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ShoppingReminder.Repository
{
    public class PurchaseRepository
    {
        SQLiteConnection db;
        public PurchaseRepository(string fileName)
        {
            string dbPath = DependencyService.Get<ISQLite>().GetDatabasePath(fileName);
            db = new SQLiteConnection(dbPath);
            db.CreateTable<Purchase>();
            db.CreateTable<SerializedHistoryItem>();
        }
        ///******************покупки*********************//
        public void ClearPurchases()
        {            
            db.DropTable<Purchase>();
            db.CreateTable<Purchase>();
        }

        public IEnumerable<Purchase> GetPurchaseItems()
        {
            return (from i in db.Table<Purchase>() select i).ToList();

        }
        public Purchase GetPurchaseItem(int id)
        {
            return db.Get<Purchase>(id);
        }
        public int DeletePurchaseItem(int id)
        {
            return db.Delete<Purchase>(id);
        }
        public int SavePurchaseItem(Purchase item)
        {
            if (item.Id != 0)
            {
                db.Update(item);
                return item.Id;
            }
            else
            {
                return db.Insert(item);
            }
        }
        ///******************история*********************//

        public void ClearHistory()
        {
            db.DropTable<SerializedHistoryItem>();
            db.CreateTable<SerializedHistoryItem>();
        }

        public List<ListOfPurchase> GetHistoryItems()
        {
            var sh= (from i in db.Table<SerializedHistoryItem>() select i).ToList();
            var tempList = new List<ListOfPurchase>();
            foreach (var item in sh)
            {
                tempList.Add(DeserializeHistory(item));
            }
            return tempList;
        }

        public ListOfPurchase GetHistoryItem(int id)
        {
            var sh= db.Get<SerializedHistoryItem>(id);
            return DeserializeHistory(sh);
        }
        public int DeleteHistoryItem(int id)
        {
            return db.Delete<SerializedHistoryItem>(id);
        }
        public int SaveHistoryItem(ListOfPurchase item)
        {
            var temp = SerializeHistory(item);
            if (item.Id != 0)
            {
                db.Update(temp);
                return temp.Id;
            }
            else
            {
                return db.Insert(temp);
            }
        }
        ///******************сериализация/десериализация*********************//
        byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                var temp = ms.ToArray();
                return temp;
            }
        }
        Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
        SerializedHistoryItem SerializeHistory(ListOfPurchase deser)
        {
            var ser = new SerializedHistoryItem();
            ser.Id = deser.Id;
            ser.Date = deser.Date;
            ser.PurchasesList = ObjectToByteArray(deser.PurchasesList);
            ser.Check = deser.Check;//TODO: сериализация фото
            return ser;
        }
        ListOfPurchase DeserializeHistory(SerializedHistoryItem ser)
        {
            var deser = new ListOfPurchase();
            deser.Id = ser.Id;
            deser.Date = ser.Date;
            deser.PurchasesList = (List<Purchase>)(ByteArrayToObject(ser.PurchasesList));
            deser.Check = ser.Check; //TODO: десериализовать фото
            return deser;
        }



    }
}
