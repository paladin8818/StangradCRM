﻿/*
 * Сделано в SharpDevelop.
 * Пользователь: Дмитрий
 * Дата: 26.09.2016
 * Время: 10:14
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using StangradCRM.Core;
using StangradCRM.Model;
using StangradCRM.ViewModel;

namespace StangradCRM.View.Controls.DirectorControls
{
	/// <summary>
	/// Interaction logic for MainControlNewBid.xaml
	/// </summary>
	public partial class ManagerBidControlID1 : UserControl
	{
		
		CollectionViewSource viewSource = null;
		CollectionViewSource equipmentBidViewSource = new CollectionViewSource();
		CollectionViewSource complectationViewSource = new CollectionViewSource();
		CollectionViewSource buyerViewSource = new CollectionViewSource();		
		public ManagerBidControlID1(CollectionViewSource viewSource)
		{
			InitializeComponent();
			viewSource.Source = BidViewModel.instance().getCollectionByStatus((int)Classes.BidStatus.New);
			viewSource.SortDescriptions.Add(new SortDescription("Date_created", ListSortDirection.Descending));
			SetViewSources();
			
			DataContext = new
			{
				BidCollection = viewSource.View,
				EquipmentBidCollection = equipmentBidViewSource.View,
				ComplectationCollection = complectationViewSource.View,
				BuyerCollection = buyerViewSource.View
			};
			this.viewSource = viewSource;
		}
		
		//Фильтры отображени/сокрытия строк таблиц
		private void SetViewSources ()
		{	
			equipmentBidViewSource.Source = EquipmentBidViewModel.instance().Collection;
			equipmentBidViewSource.Filter += delegate(object sender, FilterEventArgs e) 
			{
				EquipmentBid equipmentBid = e.Item as EquipmentBid;
				if(equipmentBid == null) return;
				
				Bid bid = dgvBid.SelectedItem as Bid;
				if(bid == null)
				{
					e.Accepted = false;
					return;
				}
				if(bid.Id == equipmentBid.Id_bid)
				{
					e.Accepted = true;
				}
				else
				{
					e.Accepted = false;
				}
			};
			
			complectationViewSource.Source = ComplectationViewModel.instance().Collection;
			complectationViewSource.Filter += delegate(object sender, FilterEventArgs e) 
			{
				Complectation complectation = e.Item as Complectation;
				if(complectation == null) return;
				
				EquipmentBid equipmentBid = dgvEquipmentBid.SelectedItem as EquipmentBid;
				if(equipmentBid == null)
				{
					e.Accepted = false;
					return;
				}
				if(complectation.Id_equipment_bid == equipmentBid.Id)
				{
					e.Accepted = true;
				}
				else
				{
					e.Accepted = false;
				}
			};
			
			buyerViewSource.Source = BuyerViewModel.instance().Collection;
			buyerViewSource.Filter += delegate(object sender, FilterEventArgs e) 
			{
				Buyer buyer = e.Item as Buyer;
				if(buyer == null) return;
				
				Bid bid = dgvBid.SelectedItem as Bid;
				if(bid == null)
				{
					e.Accepted = false;
					return;
				}
				if(buyer.Id == bid.Id_buyer)
				{
					e.Accepted = true;
				}
				else
				{
					e.Accepted = false;
				}
			};
		}		
		
		//Поиск ---->
		
		//Вызывается при вводе данных в строку поиска, осуществляет поиск
		void TbxFastSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			BidViewModel.instance().fastSearch(tbxFastSearch.Text, (TSObservableCollection<Bid>)viewSource.Source);
			viewSource.View.Refresh();
			if(dgvBid.Items.Count > 0)
			{
				dgvBid.SelectedIndex = 0;
			}
			else
			{
				dgvBid.SelectedIndex = -1;
			}
		}
		
		//Клик по кнопке очистки строки поиска
		void BtnClearFastSearch_Click(object sender, RoutedEventArgs e)
		{
			tbxFastSearch.Text = "";
		}
		
		//Заявки ---->

		//Изменение выделенной строки в таблице заявок
		void DgvBid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			equipmentBidViewSource.View.Refresh();
			buyerViewSource.View.Refresh();
		}
		
		//Контекстное меню в таблице заявок ---->
		
		//Клик по элементу меню просмотра истории платежей
		void ContextPaymentHistory_Click(object sender, RoutedEventArgs e)
		{
			Bid bid = dgvBid.SelectedItem as Bid;
			if(bid == null) 
			{
				MessageBox.Show("Заявка не выбрана!");
				return;
			}
			PaymentHistoryWindow window = new PaymentHistoryWindow(bid);
			window.ShowDialog();
		}
		
		//Оборудование в заявке ---->		
		
		//Изменение выделенной строки в таблице оборудования в заявке		
		void DgvEquipmentBid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			complectationViewSource.View.Refresh();
		}		
		
		void ContextCopy_Click(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if(mi == null) return;
			
			TextBlock textBlock = ((ContextMenu)mi.Parent).PlacementTarget as TextBlock;
			if(textBlock == null) return;
			
			Clipboard.SetText(textBlock.Text);
		}
		
		void ContextBidFiles_Click(object sender, RoutedEventArgs e)
		{
			Bid bid = dgvBid.SelectedItem as Bid;
			if(bid == null) 
			{
				MessageBox.Show("Заявка не выбрана!");
				return;
			}
			BidFilesWindow window = new BidFilesWindow(bid);
			window.ShowDialog();
		}
	}
}