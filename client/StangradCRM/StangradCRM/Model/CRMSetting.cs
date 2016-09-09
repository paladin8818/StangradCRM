﻿/*
 * Сделано в SharpDevelop.
 * Пользователь: Дмитрий
 * Дата: 01.09.2016
 * Время: 11:22
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using StangradCRM.ViewModel;

namespace StangradCRM.Model
{
	/// <summary>
	/// Description of CRMSetting.
	/// </summary>
	public class CRMSetting : Core.Model
	{
		
		public string Setting_mashine_name { get; set; }
		public string Setting_name { get; set; }
		public string Setting_value { get; set; }
		
		public CRMSetting() {}
		
		protected override void prepareSaveData(HTTPManager.HTTPRequest http)
		{
			http.addParameter("setting_value", Setting_value);
			http.addParameter("id", Id);
		}
		
		protected override void prepareRemoveData(HTTPManager.HTTPRequest http)
		{
			throw new NotImplementedException();
		}
		
		protected override string Entity {
			get {
				return "CRMSetting";
			}
		}
		
		protected override StangradCRM.Core.IViewModel CurrentViewModel {
			get {
				return CRMSettingViewModel.instance();
			}
		}
		
		public override void replace(object o)
		{
			throw new NotImplementedException();
		}
		
		public override void raiseAllProperties()
		{
			throw new NotImplementedException();
		}
	}
}
