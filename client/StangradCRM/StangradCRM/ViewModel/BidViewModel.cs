﻿/*
 * Сделано в SharpDevelop.
 * Пользователь: Дмитрий
 * Дата: 08/15/2016
 * Время: 19:04
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;

using StangradCRM.Core;
using StangradCRM.Model;
using StangradCRMLibs;

namespace StangradCRM.ViewModel
{
	/// <summary>
	/// Description of BidViewModel.
	/// </summary>
	public class BidViewModel : Core.BaseViewModel, Core.IViewModel
	{
		private static BidViewModel _instance = null;
		private TSObservableCollection<Bid> _collection
			= new TSObservableCollection<Bid>();
		
		private DateTime loadDateTime = DateTime.Now;
		
		Dictionary<int, TSObservableCollection<Bid>> _collectionByStatus
			= new Dictionary<int, TSObservableCollection<Bid>>();
		
		TSObservableCollection<Bid> _archiveCollection;
		
		public TSObservableCollection<Bid> getCollectionByStatus(int bidStatusId)
		{
			if(!_collectionByStatus.ContainsKey(bidStatusId))
			{
				List<Bid> collection = _collection.Where(x => (x.Id_bid_status == bidStatusId) && (x.Is_archive == 0)).ToList();
				_collectionByStatus.Add(bidStatusId, new TSObservableCollection<Bid>(collection));
			}
			return _collectionByStatus[bidStatusId];
		}
		
		public void UpdateStatus (Bid bid, int oldStatus)
		{
			if(bid.Id_bid_status == oldStatus) return;
			
			if(getCollectionByStatus(oldStatus).Contains(bid))
			{
				getCollectionByStatus(oldStatus).Remove(bid);
			}
			
			if(!getCollectionByStatus(bid.Id_bid_status).Contains(bid))
			{
				getCollectionByStatus(bid.Id_bid_status).Add(bid);
			}
		}
		
		public void UpdateArchive (Bid bid)
		{
			int oldStatus = (bid.Is_archive == 0) ? 1 : 0;
			if(oldStatus == 0)
			{
				MoveToArchive(bid);
			}
			else
			{
				RemoveFromArchive(bid);
			}
		}
		
		public TSObservableCollection<Bid> getArchiveCollection ()
		{
			if(_archiveCollection == null)
			{
				List<Bid> collection = _collection.Where(x => x.Is_archive != 0).ToList();
				_archiveCollection = new TSObservableCollection<Bid>(collection);
			}
			return _archiveCollection;
		}
		
		private void RemoveFromArchive (Bid bid)
		{
			if(getArchiveCollection().Contains(bid))
			{
				getArchiveCollection().Remove(bid);
				if(!getCollectionByStatus(bid.Id_bid_status).Contains(bid))
					getCollectionByStatus(bid.Id_bid_status).Add(bid);
			}
		}
		
		private void MoveToArchive (Bid bid)
		{
			if(getCollectionByStatus(bid.Id_bid_status).Contains(bid))
			{
				getCollectionByStatus(bid.Id_bid_status).Remove(bid);
				getArchiveCollection().Add(bid);
			}
		}
		
		public TSObservableCollection<Bid> Collection
		{
			get
			{
				return _collection;
			}
			set
			{
				_collection = value;
				RaisePropertyChanged("Collection", value);
			}
		}
		
		private BidViewModel() { load(); }
		
		public static BidViewModel instance()
		{
			if(_instance == null)
			{
				_instance = new BidViewModel();
			}
			return _instance;
		}
		
		public bool @add<T>(T modelItem)
		{
			Bid bid = modelItem as Bid;
			if(bid == null)
			{
				bid.LastError = "Не удалось преобразовать входные данные.";
				return false;
			}
			Bid exist = getById(bid.Id);
			if(exist != null || _collection.Contains(bid))
			{
				bid.LastError = "Данная запись уже есть в коллекции.";
				return false;
			}
			_collection.Add(bid);
			if(!getCollectionByStatus(bid.Id_bid_status).Contains(bid) && bid.Is_archive == 0)
		   	{
				getCollectionByStatus(bid.Id_bid_status).Add(bid);
			}
			if(bid.Is_archive != 0 && !getArchiveCollection().Contains(bid))
			{
				getArchiveCollection().Add(bid);
			}
			return true;
		}
		
		public bool @remove<T>(T modelItem)
		{
			Bid bid = modelItem as Bid;
			if(bid == null) 
			{
				bid.LastError = "Не удалось преобразовать входные данные.";
				return false;
			}
			if(getCollectionByStatus(bid.Id_bid_status).Contains(bid))
			{
				getCollectionByStatus(bid.Id_bid_status).Remove(bid);
			}
			if(bid.Is_archive != 0 && getArchiveCollection().Contains(bid))
			{
				getArchiveCollection().Remove(bid);
			}
			if(!_collection.Contains(bid)) return true;
			return _collection.Remove(bid);
		}
		
		public Core.Model getItem(int id)
		{
			return _collection.Where(x => x.Id == id).FirstOrDefault();
		}
		
		public Bid getById (int id)
		{
			return _collection.Where(x => x.Id == id).FirstOrDefault();
		}
		
		public List<Bid> getBySellerId (int id_seller)
		{
			return _collection.Where(x => x.Id_seller == id_seller).ToList();
		}
	
		public List<Bid> GetByPeriod (DateTime start, DateTime end)
		{
			return _collection.Where(x => (x.Date_created >= start) && (x.Date_created <= end)).ToList();
		}
		
		public DateTime GetDateCreatedFirsBid ()
		{
			if(_collection.Count == 0) return DateTime.Now;
			return _collection.Min(x => x.Date_created);
		}
		
		public List<Bid> GetByYearAndMonth (int year, int month)
		{
			if(year == 0) return new List<Bid>();
			List<Bid> bid = _collection.Where(x => x.Date_created.Year == year).ToList();
			if(month != 0)
			{
				return bid.Where(x => x.Date_created.Month == month).ToList();
			}
			return bid;
		}
		
		public DateTime maxLastModified ()
		{
			if(_collection.Count > 0)
			{
				return _collection.Max(x => x.Last_modified);
			}
			return loadDateTime;
		}
		
		public bool IsDuplicate (Bid bid)
		{
			int count = _collection.Where(x => 
                              (x.Account == bid.Account)
                              && (x.Id_seller == bid.Id_seller)
                              && (x.Id_buyer == bid.Id_buyer)
                              && (x.Id != bid.Id)).Count();
			if(count == 0) return false;
			return true;
		}
		
		//Ф-я загрузки модифицированных данных с сервера
		private List<Bid> loadLastModified ()
		{
			HTTPManager.HTTPRequest http = HTTPManager.HTTPRequest.Create(Settings.uri);
			http.UseCookie = true;
			
			string lastModified = maxLastModified().ToString("yyyy-MM-dd HH:mm:ss");
			
			http.addParameter("entity", "Bid/lastmodified");
			http.addParameter("last_modified", lastModified);
			
			if(!http.post()) return null;
			
			ResponseParser parser = ResponseParser.Parse(http.ResponseData);
			if(!parser.NoError)
			{
				return null;
			}
			return parser.ToObject<List<Bid>>();
		}
		
		//Ф-я удаленной загрузки
		public void RemoteLoad()
		{
			//Список заявок
			List<Bid> bids = null;
			try
			{
				bids = loadLastModified();
			}
			catch
			{
				return;
			}
			if(bids == null) return;
			//Аутентификационные данные 
			Auth auth = Auth.getInstance();
			
			//Цикл по новым заявкам			
			for(int i = 0; i < bids.Count; i++)
			{
				//Новая заявка
				Bid newBid = bids[i];
				//Старая заявка
				Bid oldBid = getById(newBid.Id);
				//Если старая заявка = null
				if(oldBid == null)
				{
					/* НЕ АКТУАЛЬНО, ТАК КАК ТЕПЕРЬ МЕНЕДЖЕР ВИДИТ ЗАЯВКИ ВСЕХ ПОЛЬЗОВАТЕЛЕЙ 
					//Если текущий пользователь менеджер
					if(auth.IdRole == (int)Classes.Role.Manager)
					{
						//если код менеджера заявки == коду текущего авторизованного менеджер
						if(newBid.Id_manager == auth.Id)
						{
							//добавляем новую заявку в коллекцию
							add(newBid);
						}
						else
						{
							//иначе удаляем заявку из коллекции
							newBid.remove(true);
						}
					}
					else
					{*/
						//добавляем новую заявку в коллекцию
						add(newBid);
					//}
				}
				else //если старая заявка не null
				{
					//Если текущий пользователь менеджер
					if(auth.IdRole == (int)Classes.Role.Manager)
					{
						//если код менеджера заявки == коду текущего авторизованного менеджер
						if(newBid.Id_manager == auth.Id)
						{
							//Заменяем данные старой завки на данные новой заявки
							oldBid.replace(newBid);
						}
						else
						{
							//иначе удаляем заявку
							newBid.remove(true);
						}
					}
					else
					{
						//Заменяем данные старой завки на данные новой заявки
						oldBid.replace(newBid);
					}
				}
			}
		}
		
		
		public void fastSearch (string searchString, TSObservableCollection<Bid> collection = null)
		{
			searchString = searchString.ToLower();
			string[] properties = new string[] 
			{
				"Account", "Amount", "BuyerName", "EquipmentBidStringSearch", "ManagerName"
			};
			
			if(collection == null) collection = _collection;
			
            collection.ToList().ForEach(x => x.setFilters(properties, false));
            
            collection.Where(x => (x.Account.ToLower().IndexOf(searchString) != -1) |
                             (x.Id.ToString().ToLower().IndexOf(searchString) != -1) |
                             	(x.Amount.ToString().ToLower().IndexOf(searchString) != -1) |
                             	(x.BuyerInfo.ToLower().IndexOf(searchString) != -1 ) |
                             	(x.EquipmentBidStringSearch.ToLower().IndexOf(searchString) != -1 ) |
                             	(x.ManagerName.ToLower().IndexOf(searchString) != -1)
                             ).ToList().ForEach(y => y.setFilters(properties, true));
		}
		
		protected override void removeAllItems()
		{
			_collection.ToList().ForEach(x => { remove(x); x = null; });
		}
		
		protected override void load()
		{
			EquipmentBidViewModel.instance().reload();
			PaymentViewModel.instance().reload();
			BidFilesViewModel.instance().reload();
			
			TSObservableCollection<Bid> collection =
			StangradCRM.Core.Model.load<TSObservableCollection<Bid>>("Bid");

			if(collection != default(TSObservableCollection<Bid>))
			{
				collection.ToList().ForEach(x => { x.IsSaved = true; x.loadedItemInitProperty(); add(x); });
			}
		}
	}
}
