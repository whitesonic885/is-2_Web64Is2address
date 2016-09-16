using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;

namespace is2address
{
	/// <summary>
	/// Service1 の概要の説明です。
	/// </summary>
	//--------------------------------------------------------------------------
	// 修正履歴
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 東都）高木 オブジェクトの破棄
	//	disposeReader(reader);
	//	reader = null;
	//--------------------------------------------------------------------------
	// DEL 2007.05.10 東都）高木 未使用関数のコメント化
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// MOD 2009.06.23 東都）高木 住所マスタのプライマリーキー変更 
	//--------------------------------------------------------------------------
	// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ）
	//--------------------------------------------------------------------------
	// MOD 2010.01.29 パソ）藤井 市区町村レベルの表示項目の追加（都道府県カナ名・都道府県ＣＤ）
	//--------------------------------------------------------------------------
	// MOD 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
	//--------------------------------------------------------------------------

	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2address")]

	public class Service1 : is2common.CommService
	{
		public Service1()
		{
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();

			connectService();
		}

		#region コンポーネント デザイナで生成されたコード 
		
		//Web サービス デザイナで必要です。
		private IContainer components = null;
				
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

// ADD 2005.06.07 東都）高木 都道府県選択の変更 START
		/*********************************************************************
		 * 端末マスタの都道府県ＣＤ更新
		 * 引数：端末ＣＤ、都道府県ＣＤ
		 * 戻値：なし
		 *
		 *********************************************************************/
		private void Upd_Ken(string[] sUser, OracleConnection conn2, string s端末ＣＤ, string   s都道府県ＣＤ)
		{
			string sMsg = "";

			logWriter(sUser, INF, "端末マスタの都道府県ＣＤ更新");

			OracleTransaction tran = conn2.BeginTransaction();
			string sQuery = "";
			try
			{
				// 端末マスタの更新
				sQuery  = "UPDATE ＣＭ０３端末 \n"
						+   " SET 都道府県ＣＤ = '" + s都道府県ＣＤ + "' \n"
						+ " WHERE 端末ＣＤ = '" + s端末ＣＤ + "' \n"
				;
				CmdUpdate(sUser, conn2, sQuery);

				tran.Commit();
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sMsg = chgDBErrMsg(sUser, ex);
				logWriter(sUser, ERR, sMsg);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sMsg = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sMsg);
			}

			return;
		}
// ADD 2005.06.07 東都）高木 都道府県選択の変更 END

		/*********************************************************************
		 * 市区町村一覧の取得
		 * 引数：都道府県ＣＤ
		 * 戻値：ステータス、市区町村リスト
		 *
		 *********************************************************************/
		private static string GET_BYKEN_SELECT
// MOD 2010.01.29 パソ）藤井 市区町村レベルの表示項目の追加 START
// MOD 2005.06.07 東都）高木 ORA-03113対策？ START
//			= "SELECT TRIM(市区町村名), 市区町村ＣＤ \n"
//			= "SELECT 市区町村名, 市区町村ＣＤ \n"
			= "SELECT 市区町村名, 市区町村ＣＤ, 市区町村カナ名, 都道府県ＣＤ\n"
// MOD 2005.06.07 東都）高木 ORA-03113対策？ END
// MOD 2010.01.29 パソ）藤井 市区町村レベルの表示項目の追加 END
			+   "FROM ＣＭ１２市区町村 \n";

		private static string GET_BYKEN_ORDER
			=   " AND 削除ＦＧ = '0' \n"
			+ " ORDER BY 市区町村ＣＤ \n";

		[WebMethod]
		public String[] Get_byKen(string[] sUser, string s都道府県ＣＤ)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "市区町村一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

//			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKEN_SELECT);
				sbQuery.Append(" WHERE 都道府県ＣＤ = '" + s都道府県ＣＤ + "' \n");
				sbQuery.Append(GET_BYKEN_ORDER);
//				logWriter(sUser, ERR, sbQuery.ToString());//fj
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("||" + reader.GetString(0).Trim());
					sbRet.Append("|" + reader.GetString(1).Trim());
// MOD 2010.01.29 パソ）藤井 市区町村レベルの表示項目の追加 START
//					sbRet.Append("|");
					sbRet.Append("|" + reader.GetString(2).Trim());		//都道府県名カナ
					sbRet.Append("|" + s都道府県ＣＤ);					//都道府県ＣＤ
					sbRet.Append(reader.GetString(1).Trim());			//市区町村ＣＤ
					sbRet.Append("|" );									//店所ＣＤ（なし）
// MOD 2010.01.29 パソ）藤井 市区町村レベルの表示項目の追加 END

					sList.Add(sbRet);

				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0)
				{
					sRet[0] = "該当データがありません";
				}
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

// ADD 2005.06.07 東都）高木 都道府県選択の変更 START
				// デモユーザ対応
				if(sUser[0] == "demo")
					Upd_Ken(sUser, conn2, "demo", s都道府県ＣＤ);
				else
					Upd_Ken(sUser, conn2, sUser[2], s都道府県ＣＤ);
// ADD 2005.06.07 東都）高木 都道府県選択の変更 END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}

			return sRet;
		}

		/*********************************************************************
		 * 大字通称名一覧の取得
		 * 引数：都道府県ＣＤ、市区町村ＣＤ
		 * 戻値：ステータス、大字通称名一覧
		 *
		 *********************************************************************/
		private static string GET_BYKENSHI_SELECT
// MOD 2005.06.07 東都）高木 ORA-03113対策？ START
//			= "SELECT 郵便番号, TRIM(大字通称名) \n"
// MOD 2009.06.23 東都）高木 住所マスタのプライマリーキー変更 START
//			= "SELECT 郵便番号, 大字通称名 \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） START
//			= "SELECT MAX(郵便番号), MAX(大字通称名) \n"
			= "SELECT MAX(郵便番号), 大字通称名, 大字通称カナ名, MAX(都道府県ＣＤ), MAX(市区町村ＣＤ), 大字通称ＣＤ, MAX(店所ＣＤ) \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） END
// MOD 2009.06.23 東都）高木 住所マスタのプライマリーキー変更 END
// MOD 2005.06.07 東都）高木 ORA-03113対策？ END
			+   "FROM ＣＭ１３住所 \n";

		private static string GET_BYKENSHI_ORDER
			=    "AND 削除ＦＧ = '0' \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） START
//// MOD 2009.06.23 東都）高木 住所マスタのプライマリーキー変更 START
//			+  "GROUP BY 大字通称ＣＤ \n"
//// MOD 2009.06.23 東都）高木 住所マスタのプライマリーキー変更 END
//			+  "ORDER BY 大字通称ＣＤ \n"
			+  "GROUP BY 大字通称ＣＤ,大字通称名,大字通称カナ名 \n"
			+  "ORDER BY 大字通称カナ名, 大字通称ＣＤ \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） END
			;

		[WebMethod]
		public String[] Get_byKenShi(string[] sUser, string s都道府県ＣＤ, string s市区町村ＣＤ)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "大字通称名一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

//			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKENSHI_SELECT);
				sbQuery.Append(" WHERE 都道府県ＣＤ = '" + s都道府県ＣＤ + "' \n");
				sbQuery.Append("   AND 市区町村ＣＤ = '" + s市区町村ＣＤ + "' \n");
				sbQuery.Append(GET_BYKENSHI_ORDER);
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));
					sbRet.Append("|" + reader.GetString(1).Trim());
					sbRet.Append("|D" + "|");
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） START
					sbRet.Append(reader.GetString(2).Trim());		// 大字通称カナ名
					sbRet.Append("|" + reader.GetString(3).Trim());	// 都道府県ＣＤ
					sbRet.Append(reader.GetString(4).Trim());		// 市区町村ＣＤ
					sbRet.Append(reader.GetString(5).Trim());		// 大字通称ＣＤ
					sbRet.Append("|" + reader.GetString(6).Trim());	// 店所ＣＤ
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}

			return sRet;
		}

		/*********************************************************************
		 * 住所一覧の取得
		 * 引数：郵便番号
		 * 戻値：ステータス、住所一覧
		 *
		 *********************************************************************/
		private static string GET_BYPOSTCODE_SELECT
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//			= "SELECT 郵便番号, TRIM(都道府県名), TRIM(市区町村名), TRIM(大字通称名) \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） START
//			= "SELECT 郵便番号, 都道府県名, 市区町村名, 大字通称名 \n"
			= "SELECT 郵便番号, 都道府県名, 市区町村名, 大字通称名, 大字通称カナ名, 都道府県ＣＤ, 市区町村ＣＤ, 大字通称ＣＤ, 店所ＣＤ \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） END
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
			+  " FROM ＣＭ１３住所 \n";

		private static string GET_BYPOSTCODE_ORDER
			=    "AND 削除ＦＧ = '0' \n"
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） START
//			+  "ORDER BY 郵便番号 \n";
			+  "ORDER BY 郵便番号, 大字通称カナ名 \n";
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） END



		[WebMethod]
		public String[] Get_byPostcode(string[] sUser, string s郵便番号)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "住所一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYPOSTCODE_SELECT);
				if(s郵便番号.Length == 7)
				{
					sbQuery.Append(" WHERE 郵便番号 = '" + s郵便番号 + "' ");
				}
				else
				{
					sbQuery.Append(" WHERE 郵便番号 LIKE '" + s郵便番号 + "%' ");
				}
				sbQuery.Append(GET_BYPOSTCODE_ORDER);

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));		// 郵便番号
					sbRet.Append("|" + reader.GetString(1).Trim());	// 都道府県名
					sbRet.Append(reader.GetString(2).Trim());		// 市区町村名
					sbRet.Append(reader.GetString(3).Trim());		// 大字通称名
					sbRet.Append("|D" + "|");
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） START
					sbRet.Append(reader.GetString(4).Trim());		// 大字通称カナ名
					sbRet.Append("|" + reader.GetString(5).Trim());	// 都道府県ＣＤ
					sbRet.Append(reader.GetString(6).Trim());		// 市区町村ＣＤ
					sbRet.Append(reader.GetString(7).Trim());		// 大字通称ＣＤ
					sbRet.Append("|" + reader.GetString(8).Trim());	// 店所ＣＤ
// MOD 2009.07.08 パソ）藤井 表示項目の追加（大字通称カナ・住所ＣＤ・店所ＣＤ） END
					sList.Add(sbRet);

				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}

			return sRet;
		}

		/*********************************************************************
		 * 住所の取得
		 * 引数：郵便番号
		 * 戻値：ステータス、郵便番号、住所、住所ＣＤ
		 *
		 *********************************************************************/
// ADD 2005.05.11 東都）高木 ORA-03113対策？ START
		private static string GET_BYPOSTCODE2_SELECT
			= "SELECT 郵便番号, 都道府県名, 市区町村名, 町域名, \n"
			+ " 都道府県ＣＤ, 市区町村ＣＤ, 大字通称ＣＤ \n"
			+ " FROM ＣＭ１４郵便番号 \n";
// ADD 2005.05.11 東都）高木 ORA-03113対策？ END
		[WebMethod]
		public String[] Get_byPostcode2(string[] sUser, string s郵便番号)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "住所取得開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ADD-S 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）小童谷 会員チェック追加 END

			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				cmdQuery
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					= "SELECT 郵便番号, TRIM(都道府県名), TRIM(市区町村名), TRIM(町域名), \n"
//					+        "都道府県ＣＤ || 市区町村ＣＤ || 大字通称ＣＤ \n"
//					+   " FROM ＣＭ１４郵便番号 \n";
					= GET_BYPOSTCODE2_SELECT;
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
				if(s郵便番号.Length == 7)
				{
					cmdQuery += " WHERE 郵便番号 = '" + s郵便番号 + "' \n";
				}
				else
				{
					cmdQuery += " WHERE 郵便番号 LIKE '" + s郵便番号 + "%' \n";
				}
				cmdQuery +=    " AND 削除ＦＧ = '0' \n";

				// MOD-S 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）
				//OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				logWriter(sUser, INF_SQL, "###バインド後（想定）###\n" + cmdQuery);	//修正前のUPDATE文をログ出力

				cmdQuery = GET_BYPOSTCODE2_SELECT;
				if(s郵便番号.Length == 7)
				{
					cmdQuery += " WHERE 郵便番号 = :p_YuubinNo \n";
				}
				else
				{
					cmdQuery += " WHERE 郵便番号 LIKE :p_YuubinNo \n";
				}
				cmdQuery +=    " AND 削除ＦＧ = '0' \n";

				wk_opOraParam = new OracleParameter[1];
				if(s郵便番号.Length == 7)
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s郵便番号, ParameterDirection.Input);
				}
				else
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s郵便番号+"%", ParameterDirection.Input);
				}

				OracleDataReader	reader = CmdSelect(sUser, conn2, cmdQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)横山 Oracleサーバ負荷軽減対策（SQLにバインド変数を利用）

				if (reader.Read())
				{
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					sRet[1] = reader.GetString(0);	// 郵便番号
//					sRet[2] = reader.GetString(1)	// 都道府県名
//							+ reader.GetString(2)	// 市区町村名
//							+ reader.GetString(3);	// 町域名
//					sRet[3] = reader.GetString(4);	// 住所ＣＤ
					sRet[1] = reader.GetString(0).Trim();	// 郵便番号
					sRet[2] = reader.GetString(1).Trim()	// 都道府県名
							+ reader.GetString(2).Trim()	// 市区町村名
							+ reader.GetString(3).Trim();	// 町域名
					sRet[3] = reader.GetString(4).Trim()	// 都道府県ＣＤ
							+ reader.GetString(5).Trim()	// 市区町村ＣＤ
							+ reader.GetString(6).Trim();	// 大字通称ＣＤ
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
					sRet[0] = "正常終了";
				}else{
					sRet[0] = "該当データがありません";
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}

			return sRet;
		}
	}
}
