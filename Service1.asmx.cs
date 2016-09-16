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
	/// Service1 �̊T�v�̐����ł��B
	/// </summary>
	//--------------------------------------------------------------------------
	// �C������
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j��
	//	disposeReader(reader);
	//	reader = null;
	//--------------------------------------------------------------------------
	// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// MOD 2009.06.23 ���s�j���� �Z���}�X�^�̃v���C�}���[�L�[�ύX 
	//--------------------------------------------------------------------------
	// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j
	//--------------------------------------------------------------------------
	// MOD 2010.01.29 �p�\�j���� �s�撬�����x���̕\�����ڂ̒ǉ��i�s���{���J�i���E�s���{���b�c�j
	//--------------------------------------------------------------------------
	// MOD 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
	//--------------------------------------------------------------------------

	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2address")]

	public class Service1 : is2common.CommService
	{
		public Service1()
		{
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			connectService();
		}

		#region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
		
		//Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
		private IContainer components = null;
				
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
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

// ADD 2005.06.07 ���s�j���� �s���{���I���̕ύX START
		/*********************************************************************
		 * �[���}�X�^�̓s���{���b�c�X�V
		 * �����F�[���b�c�A�s���{���b�c
		 * �ߒl�F�Ȃ�
		 *
		 *********************************************************************/
		private void Upd_Ken(string[] sUser, OracleConnection conn2, string s�[���b�c, string   s�s���{���b�c)
		{
			string sMsg = "";

			logWriter(sUser, INF, "�[���}�X�^�̓s���{���b�c�X�V");

			OracleTransaction tran = conn2.BeginTransaction();
			string sQuery = "";
			try
			{
				// �[���}�X�^�̍X�V
				sQuery  = "UPDATE �b�l�O�R�[�� \n"
						+   " SET �s���{���b�c = '" + s�s���{���b�c + "' \n"
						+ " WHERE �[���b�c = '" + s�[���b�c + "' \n"
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
				sMsg = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sMsg);
			}

			return;
		}
// ADD 2005.06.07 ���s�j���� �s���{���I���̕ύX END

		/*********************************************************************
		 * �s�撬���ꗗ�̎擾
		 * �����F�s���{���b�c
		 * �ߒl�F�X�e�[�^�X�A�s�撬�����X�g
		 *
		 *********************************************************************/
		private static string GET_BYKEN_SELECT
// MOD 2010.01.29 �p�\�j���� �s�撬�����x���̕\�����ڂ̒ǉ� START
// MOD 2005.06.07 ���s�j���� ORA-03113�΍�H START
//			= "SELECT TRIM(�s�撬����), �s�撬���b�c \n"
//			= "SELECT �s�撬����, �s�撬���b�c \n"
			= "SELECT �s�撬����, �s�撬���b�c, �s�撬���J�i��, �s���{���b�c\n"
// MOD 2005.06.07 ���s�j���� ORA-03113�΍�H END
// MOD 2010.01.29 �p�\�j���� �s�撬�����x���̕\�����ڂ̒ǉ� END
			+   "FROM �b�l�P�Q�s�撬�� \n";

		private static string GET_BYKEN_ORDER
			=   " AND �폜�e�f = '0' \n"
			+ " ORDER BY �s�撬���b�c \n";

		[WebMethod]
		public String[] Get_byKen(string[] sUser, string s�s���{���b�c)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�s�撬���ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

//			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKEN_SELECT);
				sbQuery.Append(" WHERE �s���{���b�c = '" + s�s���{���b�c + "' \n");
				sbQuery.Append(GET_BYKEN_ORDER);
//				logWriter(sUser, ERR, sbQuery.ToString());//fj
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("||" + reader.GetString(0).Trim());
					sbRet.Append("|" + reader.GetString(1).Trim());
// MOD 2010.01.29 �p�\�j���� �s�撬�����x���̕\�����ڂ̒ǉ� START
//					sbRet.Append("|");
					sbRet.Append("|" + reader.GetString(2).Trim());		//�s���{�����J�i
					sbRet.Append("|" + s�s���{���b�c);					//�s���{���b�c
					sbRet.Append(reader.GetString(1).Trim());			//�s�撬���b�c
					sbRet.Append("|" );									//�X���b�c�i�Ȃ��j
// MOD 2010.01.29 �p�\�j���� �s�撬�����x���̕\�����ڂ̒ǉ� END

					sList.Add(sbRet);

				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0)
				{
					sRet[0] = "�Y���f�[�^������܂���";
				}
				else
				{
					sRet[0] = "����I��";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}

// ADD 2005.06.07 ���s�j���� �s���{���I���̕ύX START
				// �f�����[�U�Ή�
				if(sUser[0] == "demo")
					Upd_Ken(sUser, conn2, "demo", s�s���{���b�c);
				else
					Upd_Ken(sUser, conn2, sUser[2], s�s���{���b�c);
// ADD 2005.06.07 ���s�j���� �s���{���I���̕ύX END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}

			return sRet;
		}

		/*********************************************************************
		 * �厚�ʏ̖��ꗗ�̎擾
		 * �����F�s���{���b�c�A�s�撬���b�c
		 * �ߒl�F�X�e�[�^�X�A�厚�ʏ̖��ꗗ
		 *
		 *********************************************************************/
		private static string GET_BYKENSHI_SELECT
// MOD 2005.06.07 ���s�j���� ORA-03113�΍�H START
//			= "SELECT �X�֔ԍ�, TRIM(�厚�ʏ̖�) \n"
// MOD 2009.06.23 ���s�j���� �Z���}�X�^�̃v���C�}���[�L�[�ύX START
//			= "SELECT �X�֔ԍ�, �厚�ʏ̖� \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j START
//			= "SELECT MAX(�X�֔ԍ�), MAX(�厚�ʏ̖�) \n"
			= "SELECT MAX(�X�֔ԍ�), �厚�ʏ̖�, �厚�ʏ̃J�i��, MAX(�s���{���b�c), MAX(�s�撬���b�c), �厚�ʏ̂b�c, MAX(�X���b�c) \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j END
// MOD 2009.06.23 ���s�j���� �Z���}�X�^�̃v���C�}���[�L�[�ύX END
// MOD 2005.06.07 ���s�j���� ORA-03113�΍�H END
			+   "FROM �b�l�P�R�Z�� \n";

		private static string GET_BYKENSHI_ORDER
			=    "AND �폜�e�f = '0' \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j START
//// MOD 2009.06.23 ���s�j���� �Z���}�X�^�̃v���C�}���[�L�[�ύX START
//			+  "GROUP BY �厚�ʏ̂b�c \n"
//// MOD 2009.06.23 ���s�j���� �Z���}�X�^�̃v���C�}���[�L�[�ύX END
//			+  "ORDER BY �厚�ʏ̂b�c \n"
			+  "GROUP BY �厚�ʏ̂b�c,�厚�ʏ̖�,�厚�ʏ̃J�i�� \n"
			+  "ORDER BY �厚�ʏ̃J�i��, �厚�ʏ̂b�c \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j END
			;

		[WebMethod]
		public String[] Get_byKenShi(string[] sUser, string s�s���{���b�c, string s�s�撬���b�c)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�厚�ʏ̖��ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

//			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYKENSHI_SELECT);
				sbQuery.Append(" WHERE �s���{���b�c = '" + s�s���{���b�c + "' \n");
				sbQuery.Append("   AND �s�撬���b�c = '" + s�s�撬���b�c + "' \n");
				sbQuery.Append(GET_BYKENSHI_ORDER);
				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));
					sbRet.Append("|" + reader.GetString(1).Trim());
					sbRet.Append("|D" + "|");
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j START
					sbRet.Append(reader.GetString(2).Trim());		// �厚�ʏ̃J�i��
					sbRet.Append("|" + reader.GetString(3).Trim());	// �s���{���b�c
					sbRet.Append(reader.GetString(4).Trim());		// �s�撬���b�c
					sbRet.Append(reader.GetString(5).Trim());		// �厚�ʏ̂b�c
					sbRet.Append("|" + reader.GetString(6).Trim());	// �X���b�c
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}

			return sRet;
		}

		/*********************************************************************
		 * �Z���ꗗ�̎擾
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�Z���ꗗ
		 *
		 *********************************************************************/
		private static string GET_BYPOSTCODE_SELECT
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//			= "SELECT �X�֔ԍ�, TRIM(�s���{����), TRIM(�s�撬����), TRIM(�厚�ʏ̖�) \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j START
//			= "SELECT �X�֔ԍ�, �s���{����, �s�撬����, �厚�ʏ̖� \n"
			= "SELECT �X�֔ԍ�, �s���{����, �s�撬����, �厚�ʏ̖�, �厚�ʏ̃J�i��, �s���{���b�c, �s�撬���b�c, �厚�ʏ̂b�c, �X���b�c \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j END
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
			+  " FROM �b�l�P�R�Z�� \n";

		private static string GET_BYPOSTCODE_ORDER
			=    "AND �폜�e�f = '0' \n"
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j START
//			+  "ORDER BY �X�֔ԍ� \n";
			+  "ORDER BY �X�֔ԍ�, �厚�ʏ̃J�i�� \n";
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j END



		[WebMethod]
		public String[] Get_byPostcode(string[] sUser, string s�X�֔ԍ�)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�Z���ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_BYPOSTCODE_SELECT);
				if(s�X�֔ԍ�.Length == 7)
				{
					sbQuery.Append(" WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' ");
				}
				else
				{
					sbQuery.Append(" WHERE �X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' ");
				}
				sbQuery.Append(GET_BYPOSTCODE_ORDER);

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);

				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

					sbRet.Append("|" + reader.GetString(0));		// �X�֔ԍ�
					sbRet.Append("|" + reader.GetString(1).Trim());	// �s���{����
					sbRet.Append(reader.GetString(2).Trim());		// �s�撬����
					sbRet.Append(reader.GetString(3).Trim());		// �厚�ʏ̖�
					sbRet.Append("|D" + "|");
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j START
					sbRet.Append(reader.GetString(4).Trim());		// �厚�ʏ̃J�i��
					sbRet.Append("|" + reader.GetString(5).Trim());	// �s���{���b�c
					sbRet.Append(reader.GetString(6).Trim());		// �s�撬���b�c
					sbRet.Append(reader.GetString(7).Trim());		// �厚�ʏ̂b�c
					sbRet.Append("|" + reader.GetString(8).Trim());	// �X���b�c
// MOD 2009.07.08 �p�\�j���� �\�����ڂ̒ǉ��i�厚�ʏ̃J�i�E�Z���b�c�E�X���b�c�j END
					sList.Add(sbRet);

				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}

			return sRet;
		}

		/*********************************************************************
		 * �Z���̎擾
		 * �����F�X�֔ԍ�
		 * �ߒl�F�X�e�[�^�X�A�X�֔ԍ��A�Z���A�Z���b�c
		 *
		 *********************************************************************/
// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H START
		private static string GET_BYPOSTCODE2_SELECT
			= "SELECT �X�֔ԍ�, �s���{����, �s�撬����, ���於, \n"
			+ " �s���{���b�c, �s�撬���b�c, �厚�ʏ̂b�c \n"
			+ " FROM �b�l�P�S�X�֔ԍ� \n";
// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H END
		[WebMethod]
		public String[] Get_byPostcode2(string[] sUser, string s�X�֔ԍ�)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�Z���擾�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[4];
			// ADD-S 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
			OracleParameter[]	wk_opOraParam	= null;
			// ADD-E 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�����J ����`�F�b�N�ǉ� END

			string cmdQuery = "";
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				cmdQuery
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					= "SELECT �X�֔ԍ�, TRIM(�s���{����), TRIM(�s�撬����), TRIM(���於), \n"
//					+        "�s���{���b�c || �s�撬���b�c || �厚�ʏ̂b�c \n"
//					+   " FROM �b�l�P�S�X�֔ԍ� \n";
					= GET_BYPOSTCODE2_SELECT;
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
				if(s�X�֔ԍ�.Length == 7)
				{
					cmdQuery += " WHERE �X�֔ԍ� = '" + s�X�֔ԍ� + "' \n";
				}
				else
				{
					cmdQuery += " WHERE �X�֔ԍ� LIKE '" + s�X�֔ԍ� + "%' \n";
				}
				cmdQuery +=    " AND �폜�e�f = '0' \n";

				// MOD-S 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j
				//OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
				logWriter(sUser, INF_SQL, "###�o�C���h��i�z��j###\n" + cmdQuery);	//�C���O��UPDATE�������O�o��

				cmdQuery = GET_BYPOSTCODE2_SELECT;
				if(s�X�֔ԍ�.Length == 7)
				{
					cmdQuery += " WHERE �X�֔ԍ� = :p_YuubinNo \n";
				}
				else
				{
					cmdQuery += " WHERE �X�֔ԍ� LIKE :p_YuubinNo \n";
				}
				cmdQuery +=    " AND �폜�e�f = '0' \n";

				wk_opOraParam = new OracleParameter[1];
				if(s�X�֔ԍ�.Length == 7)
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s�X�֔ԍ�, ParameterDirection.Input);
				}
				else
				{
					wk_opOraParam[0] = new OracleParameter("p_YuubinNo", OracleDbType.Char, s�X�֔ԍ�+"%", ParameterDirection.Input);
				}

				OracleDataReader	reader = CmdSelect(sUser, conn2, cmdQuery, wk_opOraParam);
				wk_opOraParam = null;
				// MOD-E 2012.09.06 COA)���R Oracle�T�[�o���׌y���΍�iSQL�Ƀo�C���h�ϐ��𗘗p�j

				if (reader.Read())
				{
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					sRet[1] = reader.GetString(0);	// �X�֔ԍ�
//					sRet[2] = reader.GetString(1)	// �s���{����
//							+ reader.GetString(2)	// �s�撬����
//							+ reader.GetString(3);	// ���於
//					sRet[3] = reader.GetString(4);	// �Z���b�c
					sRet[1] = reader.GetString(0).Trim();	// �X�֔ԍ�
					sRet[2] = reader.GetString(1).Trim()	// �s���{����
							+ reader.GetString(2).Trim()	// �s�撬����
							+ reader.GetString(3).Trim();	// ���於
					sRet[3] = reader.GetString(4).Trim()	// �s���{���b�c
							+ reader.GetString(5).Trim()	// �s�撬���b�c
							+ reader.GetString(6).Trim();	// �厚�ʏ̂b�c
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
					sRet[0] = "����I��";
				}else{
					sRet[0] = "�Y���f�[�^������܂���";
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				logWriter(sUser, INF, sRet[0]);
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}

			return sRet;
		}
	}
}
