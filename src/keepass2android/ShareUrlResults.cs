/*
This file is part of Keepass2Android, Copyright 2013 Philipp Crocoll. 

  Keepass2Android is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 2 of the License, or
  (at your option) any later version.

  Keepass2Android is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with Keepass2Android.  If not, see <http://www.gnu.org/licenses/>.
  */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using keepass2android.view;

namespace keepass2android
{
	[Activity (Label = "@string/kp2a_findUrl")]		
	public class ShareUrlResults : GroupBaseActivity
	{

		public ShareUrlResults (IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
			
		}

		public ShareUrlResults()
		{
		}

		public static void Launch(Activity act, string urlToSearchFor)
		{
			Intent i = new Intent(act, typeof(ShareUrlResults));
			i.PutExtra(Intent.ExtraText, urlToSearchFor);
			act.StartActivityForResult(i, 0);

		}


		private Database mDb;

		
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetResult(KeePass.EXIT_CLOSE_AFTER_SEARCH);
			
			mDb = App.getDB();

			String searchUrl = getSearchUrl(Intent);
			
			if (!mDb.Loaded)
			{
				Intent intent = new Intent(this, typeof(FileSelectActivity));
				intent.PutExtra(FileSelectActivity.UrlToSearch_key, searchUrl);
				StartActivityForResult(intent, 0);

				Finish();
			}
			else if (mDb.Locked)
			{
				PasswordActivity.Launch(this,mDb.mIoc, searchUrl);
				Finish();
			}
			else
			{
				query(searchUrl);
			}
			
		}

		public override void LaunchActivityForEntry(KeePassLib.PwEntry pwEntry, int pos)
		{
			EntryActivity.Launch(this, pwEntry, pos, true);
			Finish();
		}
		
		private void query(String url)
		{
			//first: search for exact url
			try
			{
				mGroup = mDb.SearchForExactUrl(url);
			} catch (Exception e)
			{
				Toast.MakeText(this, e.Message, ToastLength.Long).Show();
				Finish();
				return;
			}
			//if no results, search for host (e.g. "accounts.google.com")
			if (mGroup.Entries.Count() == 0)
			{
				try
				{
					mGroup = mDb.SearchForHost(url, false);
				} catch (Exception e)
				{
					Toast.MakeText(this, e.Message, ToastLength.Long).Show();
					Finish();
					return;
				}
			}
			//if still no results, search for host, allowing subdomains ("www.google.com" in entry is ok for "accounts.google.com" in search (but not the other way around)
			if (mGroup.Entries.Count() == 0)
			{
				try
				{
					mGroup = mDb.SearchForHost(url, true);
				} catch (Exception e)
				{
					Toast.MakeText(this, e.Message, ToastLength.Long).Show();
					Finish();
					return;
				}
			}
			
			
			if (mGroup == null || (mGroup.Entries.Count() < 1))
			{
				SetContentView(new GroupEmptyView(this));
			} else
			{
				SetContentView(new GroupViewOnlyView(this));
			}
			
			setGroupTitle();
			
			ListAdapter = new PwGroupListAdapter(this, mGroup);

			if (mGroup.Entries.Count() == 1)
			{
				LaunchActivityForEntry(mGroup.Entries.Single(),0);
			}
		}
		
		private String getSearchUrl(Intent queryIntent) {
			String queryAction = queryIntent.Action;
			return queryIntent.GetStringExtra(Intent.ExtraText);
			
		}

		public override bool OnSearchRequested()
		{
			if (base.OnSearchRequested())
			{
				Finish();
				return true;
			} else
			{
				return false;
			}
		}
	}}

