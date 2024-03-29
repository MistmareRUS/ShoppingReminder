﻿using ShoppingReminder.Model;
using SQLite;
using System.Linq;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ShoppingReminder.ViewModel;

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
            db.CreateTable<Plan>();
            db.CreateTable<SerializedGroup>();
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
        ///******************планы*********************//
        public void ClearPlans()
        {
            db.DropTable<Plan>();
            db.CreateTable<Plan>();
        }
        public IEnumerable<Plan> GetPlanItems()
        {
            return (from i in db.Table<Plan>() select i).ToList();

        }
        public Plan GetPlanItem(int id)
        {
            return db.Get<Plan>(id);
        }
        public int DeletePlanItem(int id)
        {
            return db.Delete<Plan>(id);
        }
        public int SavePlanItem(Plan item)
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
        ///******************группы*********************//
        public void ClearGroups()
        {
            db.DropTable<SerializedGroup>();
            db.CreateTable<SerializedGroup>();
        }
        public List<GroupViewModel> GetGroupItems()
        {
            var sg = (from i in db.Table<SerializedGroup>() select i).ToList();
            var tempList = new List<GroupViewModel>();
            foreach (var item in sg)
            {
                var tempDes = DeserializeGroup(item);
                var tempGVM = new GroupViewModel(null) { Group=tempDes };
                tempList.Add(tempGVM);
            }
            return tempList;
        }
        public Group GetGroupItem(int id)
        {
            var sg = db.Get<SerializedGroup>(id);
            return DeserializeGroup(sg);
        }
        public int DeleteGroupItem(int id)
        {
            return db.Delete<SerializedGroup>(id);
        }
        public int SaveGroupItem(Group item)
        {
            Group group = item;
            var temp = SerializeGroup(group);
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
        ///******************история*********************//
        public void ClearHistory()
        {
            db.DropTable<SerializedHistoryItem>();
            db.CreateTable<SerializedHistoryItem>();
        }
        public List<HistoryViewModel> GetHistoryItems()
        {
            var sh = (from i in db.Table<SerializedHistoryItem>() select i).ToList();
            var tempList = new List<HistoryViewModel>();
            foreach (var item in sh)
            {
                var tempDes = DeserializeHistory(item);
                var tempHVM = new HistoryViewModel() { ListOfPurchase = tempDes };
                tempList.Add(tempHVM);
            }
            return tempList;
        }
        public ListOfPurchase GetHistoryItem(int id)
        {
            var sh = db.Get<SerializedHistoryItem>(id);
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
            var ser = new SerializedHistoryItem
            {
                Date = deser.Date,
                PurchasesList = ObjectToByteArray(deser.PurchasesList),
                Check = deser.Check,
                Id=deser.Id,
                ShopName=deser.ShopName
            };
            return ser;
        }
        ListOfPurchase DeserializeHistory(SerializedHistoryItem ser)
        {
            var deser = new ListOfPurchase
            {
                Id = ser.Id,
                Date = ser.Date,
                PurchasesList = (List<Purchase>)(ByteArrayToObject(ser.PurchasesList)),
                Check = ser.Check,
                ShopName=ser.ShopName
            };
            return deser;
        }
        SerializedGroup SerializeGroup(Group deser)
        {
            var ser = new SerializedGroup
            {
                PurchasesList = deser.PurchasesList!=null? ObjectToByteArray(deser.PurchasesList):null,
                Id = deser.Id,
                Name=deser.Name
            };
            return ser;
        }
        Group DeserializeGroup(SerializedGroup ser)
        {
            var deser = new Group
            {
                Id = ser.Id,
                PurchasesList = ser.PurchasesList != null ? (List<Purchase>)(ByteArrayToObject(ser.PurchasesList)) : null,
                Name=ser.Name
            };
            return deser;
        }



    }
}
