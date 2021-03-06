﻿/*
 * Сделано в SharpDevelop.
 * Пользователь: Дмитрий
 * Дата: 08/15/2016
 * Время: 19:09
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using StangradCRM.Core;
using StangradCRM.ViewModel;
using StangradCRMLibs;

namespace StangradCRM.Model
{
	/// <summary>
	/// Description of EquipmentBid.
	/// </summary>
	public class EquipmentBid : Core.Model
	{
		public int Id_equipment { get; set; }
		public int? Id_modification { get; set; }
		public int Id_bid { get; set; }
		public int? Serial_number { get; set; }
		public int Is_blank_print { get; set; }
		public int Is_archive { get; set; }
		
		public bool IsBlankPrint 
		{
			get
			{
				if(Is_blank_print == 0) return false;
				return true;
			}
		}
		
		//-------------------->
		
		public TSObservableCollection<Complectation> Complectation
		{
			set
			{
				TSObservableCollection<Complectation> complectations = value;
				ComplectationViewModel c_vm = ComplectationViewModel.instance();
				for(int i = 0; i < complectations.Count; i++)
				{
					Complectation complectation = c_vm.getById(complectations[i].Id);
					if(complectation == null)
					{
						c_vm.add(complectations[i]);
					}
					else
					{
						complectation.replace(complectations[i]);
					}
				}
			}
		}
		
		//--------------------<
		
		
		
		public EquipmentBid() {	}
		
		protected override void prepareSaveData(HTTPManager.HTTPRequest http)
		{
			
			http.addParameter("id_equipment", Id_equipment);
			http.addParameter("id_bid", Id_bid);
			http.addParameter("is_blank_print", Is_blank_print);
			http.addParameter("is_archive", Is_archive);
			if(Id != 0)
			{
				http.addParameter("id", Id);
			}
			
			if(Id_modification != null)
			{
				http.addParameter("id_modification", (int)Id_modification);
			}
			else
			{
				http.addParameter("id_modification", "NULL");
			}
			
			if(Serial_number != null)
			{
				http.addParameter("serial_number", Serial_number);
			}
		}
		
		protected override void prepareRemoveData(HTTPManager.HTTPRequest http)
		{
			if(Id != 0)
			{
				http.addParameter("id", Id);
			}
		}
		
		protected override string Entity {
			get {
				return "EquipmentBid";
			}
		}
		
		protected override StangradCRM.Core.IViewModel CurrentViewModel {
			get {
				return EquipmentBidViewModel.instance();
			}
		}
		
		
		//EXT
		
		public Modification Modification
		{
			get
			{
				if(Id_modification != null)
				{
					return ModificationViewModel.instance().getById((int)Id_modification);
				}
				return null;
			}
		}
		
		public Equipment Equipment
		{
			get
			{
				return EquipmentViewModel.instance().getById(Id_equipment);
			}
		}
		
		public string EquipmentName 
		{
			get
			{
				if(this.Equipment == null) return "<Не выбрано>";
				return this.Equipment.Name;
			}
		}
		
		public string ModificationName
		{
			get
			{
				if(this.Modification == null) return "<Не выбрано>";
				return this.Modification.Name;				
			}
		}
		
		private TSObservableCollection<Complectation> complectationCollection = null;
		public TSObservableCollection<Complectation> ComplectationCollection
		{
			get
			{
				if(complectationCollection == null)
				{
					complectationCollection = ComplectationViewModel.instance().getByEquipmentBidId(Id);
				}
				return complectationCollection;
			}
		}
		
		//Search ---->
		public string ComplectationStringSearch
		{
			get
			{
				string resultString = "";
				foreach(Complectation complectation in ComplectationCollection)
				{
					ComplectationItem item = ComplectationItemViewModel.instance().getById(complectation.Id_complectation_item);
					if(item != null)
					{
						resultString += item.Name + " ";
					}
					resultString += complectation.Complectation_count.ToString();
				}
				return resultString;
			}
		}
		
		//<----
		
		//Для тех отдела ---->
		
		public string MounthYearDateProduction
		{
			get
			{
				Bid bid = BidViewModel.instance().getById(Id_bid);
				if(bid == null || bid.Planned_shipment_date == null) return "";
				DateTime date = (DateTime)bid.Planned_shipment_date;
				string monthName = Classes.Months.getRuMonthNameByNumber(date.Month);
				string year = date.Year.ToString();
				return monthName + " " + year;
			}
		}
		
		public DateTime? PlannedShipmentDate
		{
			get
			{
				Bid bid = BidViewModel.instance().getById(Id_bid);
				if(bid == null || bid.Planned_shipment_date == null) return null;
				return bid.Planned_shipment_date;
			}
		}
		
		public string ManagerName 
		{
			get
			{
				Bid bid = BidViewModel.instance().getById(Id_bid);
				if(bid == null) return "";
				return bid.ManagerName;
			}
		}
		
		public string Account
		{
			get
			{
				Bid bid = BidViewModel.instance().getById(Id_bid);
				if(bid == null) return "";
				return bid.Account;
			}
		}
		
		// <----
		
		protected override bool afterSave(StangradCRMLibs.ResponseParser parser)
		{
			bool result = base.afterSave(parser);
			if(result)
			{
				raiseAllProperties();
				Bid bid = BidViewModel.instance().getById(Id_bid);
				if(bid != null && !bid.EquipmentBidCollection.Contains(this))
				{
					bid.EquipmentBidCollection.Add(this);
				}
			}
			return result;
		}
		
		protected override bool afterRemove(ResponseParser parser, bool soft = false)
		{
			bool result = base.afterRemove(parser, soft);
			if(result)
			{
				Bid bid = BidViewModel.instance().getById(Id_bid);
				if(bid != null && bid.EquipmentBidCollection.Contains(this))
				{
					bid.EquipmentBidCollection.Remove(this);
				}
				ComplectationViewModel.instance().getByEquipmentBidId(Id).ToList().ForEach(x => {x.remove(true);});
			}
			return result;
		}
		
		public void setSerialNumber (int serial_number)
		{
			Serial_number = serial_number;
			RaisePropertyChanged("Serial_number", Serial_number);
		}
		
		public override void replace(object o)
		{
			EquipmentBid equipmentBid = o as EquipmentBid;
			if(equipmentBid == null) return;
			Id_modification = equipmentBid.Id_modification;
			Id_bid = equipmentBid.Id_bid;
			Serial_number = equipmentBid.Serial_number;
			Is_blank_print = equipmentBid.Is_blank_print;
			Is_archive = equipmentBid.Is_archive;
			
			raiseAllProperties();
		}
		
		public override void raiseAllProperties()
		{
			if(oldValues.ContainsKey("Is_archive") &&
			   (oldValues["Is_archive"] as int?) != null &&
			   Is_archive != (int)oldValues["Is_archive"])
			{
				EquipmentBidViewModel.instance().updateStatus(this);
			}
			
			RaisePropertyChanged("Id_equipment", Id_equipment);
			RaisePropertyChanged("Id_modification", Id_modification);
			RaisePropertyChanged("Id_bid", Id_bid);
			RaisePropertyChanged("Serial_number", Serial_number);
			RaisePropertyChanged("Is_blank_print", Is_blank_print);
			RaisePropertyChanged("Is_archive", Is_archive);
			RaisePropertyChanged("EquipmentName", null);
			RaisePropertyChanged("ModificationName", null);
			RaisePropertyChanged("IsBlankPrint", null);
		}
		
		public override void loadedItemInitProperty()
		{
			base.loadedItemInitProperty();
			oldValues["Is_archive"] = Is_archive;
		}
	}
}
