using getAssyDJ.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace getAssyDJ
{
    public class COracle
    {
        String m_server;
        String m_SID;
        private String m_user;
        private String m_pass;
        OracleConnection m_OracleDB;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public string Server { get => m_server; set => m_server = value; }
        public string SID { get => m_SID; set => m_SID = value; }

        public COracle()
        {
            m_server = "192.168.0.25";
            m_SID = "SEMPROD";
            m_user = "APPS";
            m_pass = "apps";
            m_OracleDB = GetDBConnection(Server, 0, SID, m_user, m_pass);
            m_OracleDB.Open();
        }
        public COracle(String serv, String Sid)
        {
            m_server = serv;
            m_SID = Sid;
            m_user = "APPS";
            m_pass = "apps";
            m_OracleDB = GetDBConnection(Server, 0, SID, m_user, m_pass);
            m_OracleDB.Open();
        }

        private OracleConnection GetDBConnection(string host, int port, String sid, String user, String password)
        {


            Console.WriteLine("Getting Connection ...");

            // 'Connection string' to connect directly to Oracle.
            string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
                 + Server + ")(PORT = " + "1521" + "))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = "
                 + SID + ")));Password=" + m_pass + ";User ID=" + m_user + ";Enlist=false;Pooling=true";

            OracleConnection conn = new OracleConnection();
            try
            {
                conn.ConnectionString = connString;
            }
            catch (Exception ex)
            {
                conn = null;
                logger.Error(ex, "Error al conectarse a base de datos de Oracle");
            }

            return conn;
        }
        public bool QuerySerial(String serial, ref int resultTest)
        {
            bool result = false;
            string sql = "SELECT * FROM insp_result_summary_info where board_barcode in ('" + serial.ToUpper() + "')"; ;

            try
            {
                // Create command.
                OracleCommand cmd = new OracleCommand();

                // Set connection for command.
                cmd.Connection = m_OracleDB;
                cmd.CommandText = sql;


                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        result = true;

                        while (reader.Read())
                        {
                            int IRCODEIndex = reader.GetOrdinal("INSP_RESULT_CODE");
                            int VLRCODEIndex = reader.GetOrdinal("VC_LAST_RESULT_CODE");

                            long? INSP_RESULT_CODE = null;
                            long? VC_LAST_RESULT_CODE = null;

                            if (!reader.IsDBNull(IRCODEIndex))
                                INSP_RESULT_CODE = Convert.ToInt64(reader.GetValue(IRCODEIndex));
                            if (!reader.IsDBNull(VLRCODEIndex))
                                VC_LAST_RESULT_CODE = Convert.ToInt64(reader.GetValue(VLRCODEIndex));

                            if (INSP_RESULT_CODE == 0 && VC_LAST_RESULT_CODE == null)
                                resultTest = 1;   //// OK
                            if (INSP_RESULT_CODE != 0 && VC_LAST_RESULT_CODE != 0)
                                resultTest = 2;   //// NG
                            if (INSP_RESULT_CODE != 0 && VC_LAST_RESULT_CODE == 0)
                                resultTest = 3;   //// FALSE CALL (OK)
                            if (INSP_RESULT_CODE != 0 && VC_LAST_RESULT_CODE == null)
                                resultTest = 4;   //// NO JUZGADA

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[QuerySerial] Error en serial: " + serial);
                result = false;
            }
            return result;

        }

        public bool QuerySerials(List<String> serials, ref int resultTest)
        {
            bool result = false;
            String qSerials = "";

            foreach (String serial in serials)
            {
                qSerials += "'" + serial.ToUpper() + "',";
            }

            string sql = "SELECT * FROM insp_result_summary_info where board_barcode in (" + qSerials.Substring(0, qSerials.Length - 1) + ")"; ;

            try
            {
                // Create command.
                OracleCommand cmd = new OracleCommand();

                // Set connection for command.
                cmd.Connection = m_OracleDB;
                cmd.CommandText = sql;


                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        result = true;

                        while (reader.Read())
                        {
                            int IRCODEIndex = reader.GetOrdinal("INSP_RESULT_CODE");
                            int VLRCODEIndex = reader.GetOrdinal("VC_LAST_RESULT_CODE");

                            long? INSP_RESULT_CODE = null;
                            long? VC_LAST_RESULT_CODE = null;

                            if (!reader.IsDBNull(IRCODEIndex))
                                INSP_RESULT_CODE = Convert.ToInt64(reader.GetValue(IRCODEIndex));
                            if (!reader.IsDBNull(VLRCODEIndex))
                                VC_LAST_RESULT_CODE = Convert.ToInt64(reader.GetValue(VLRCODEIndex));

                            if (INSP_RESULT_CODE == 0 && VC_LAST_RESULT_CODE == null)
                                resultTest = 1;   //// OK
                            if (INSP_RESULT_CODE != 0 && VC_LAST_RESULT_CODE != 0)
                                resultTest = 2;   //// NG
                            if (INSP_RESULT_CODE != 0 && VC_LAST_RESULT_CODE == 0)
                                resultTest = 3;   //// FALSE CALL (OK)
                            if (INSP_RESULT_CODE != 0 && VC_LAST_RESULT_CODE == null)
                                resultTest = 4;   //// NO JUZGADA
                            //break;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[QuerySerial] Error ");
                result = false;
            }
            return result;

        }
        public bool getSMTDJs(String DJS, ref List<getAssyDJPicked_Result> assyDjs, int filter)
        {
            assyDjs = new List<getAssyDJPicked_Result>();
            bool result = false;

            String strFilter = "";

            if(filter==0)
                strFilter = "WHERE results.SUBASSEMBLY LIKE '%MANU%'";
            else
                strFilter = "WHERE results.SUBASSEMBLY LIKE '%SMT%'";
            String query = "SELECT * FROM " +
                         "(SELECT(SELECT DISTINCT D.ASSEMBLY_DESC FROM SIIXSEM.DJ_GROUP_DTL H INNER JOIN SIIXSEM.DJ_GROUP_DTL D ON H.DJ_ID = D.DJ_ID WHERE D.DJ_NO = e.DJ_NO) MODEL_NAME, " +
                         "        (SELECT DISTINCT D.STATUS FROM SIIXSEM.DJ_GROUP_DTL H INNER JOIN SIIXSEM.DJ_GROUP_DTL D ON H.DJ_ID = D.DJ_ID WHERE D.DJ_NO = e.DJ_NO) DJ_STATUS, " +
                         "        (SELECT DISTINCT D.COMPLETION_SUBINV FROM SIIXSEM.DJ_GROUP_HDR H INNER JOIN SIIXSEM.DJ_GROUP_DTL D ON H.DJ_ID = D.DJ_ID WHERE D.DJ_NO = e.DJ_NO) SUBINV, " +
                         "        (SELECT DISTINCT H.GROUP_NO FROM SIIXSEM.DJ_GROUP_HDR H INNER JOIN SIIXSEM.DJ_GROUP_DTL D ON H.DJ_ID = D.DJ_ID WHERE D.DJ_NO = e.DJ_NO) GROUP_NO, " +
                         "        DJ_NO,(SELECT dtln.ASSEMBLY_NAME FROM SIIXSEM.DJ_GROUP_DTL dtln WHERE dtln.DJ_NO = e.DJ_NO)SUBASSEMBLY,ITEM_CD, " +
                         "        CASE WHEN OPEN_QTY < PICKED_QTY THEN 0 ELSE OPEN_QTY -PICKED_QTY END BALANCE, OPEN_QTY CANTIDAD, PICKED_QTY PICKED, TRUNC(((PICKED_QTY / OPEN_QTY) * 100))|| '%' PORCENTAGE " +
                         " ,(SELECT NVL(SUM(PRIMARY_TRANSACTION_QUANTITY), 0) FROM INV.MTL_ONHAND_QUANTITIES_DETAIL " +
                         "                                     WHERE INVENTORY_ITEM_ID = (SELECT DISTINCT WRO.INVENTORY_ITEM_ID " +
                         "                                                FROM WIP.WIP_REQUIREMENT_OPERATIONS wro " +
                         "                                                   INNER JOIN WIP.WIP_ENTITIES we ON wro.WIP_ENTITY_ID = we.WIP_ENTITY_ID " +
                         "                                                   WHERE we.WIP_ENTITY_NAME = e.DJ_NO " +
                         "                                                   AND WRO.INVENTORY_ITEM_ID = e.ITEM_ID) " +
                         "                                       AND ORGANIZATION_ID = (SELECT DISTINCT WRO.ORGANIZATION_ID " +
                         "                                                               FROM WIP.WIP_REQUIREMENT_OPERATIONS wro " +
                         "                                                               INNER JOIN WIP.WIP_ENTITIES we ON wro.WIP_ENTITY_ID = we.WIP_ENTITY_ID " +
                         "                                                               WHERE we.WIP_ENTITY_NAME = e.DJ_NO " +
                         "                                                               AND WRO.INVENTORY_ITEM_ID = e.ITEM_ID) " +
                         "                                       AND SUBINVENTORY_CODE = 'IWH') EXISTS_IWH_QTY " +
                         "       FROM(SELECT COUNT(pl.ITEM_CD) TOTAL_ITEM " +
                         "                                   , pl.DJ_NO " +
                         "                                       , pl.ITEM_CD " +
                         "                                       , pl.ITEM_ID " +
                         "                                       , pl.OPEN_QTY " +
                         "                                       , SUM(pl.PICKED_QTY) PICKED_QTY " +
                         "               FROM SIIXSEM.DJ_MASTER_PICK_LIST pl " +
                         "               WHERE pl.DJ_NO IN(SELECT D.DJ_NO " +
                         "                                   FROM SIIXSEM.DJ_GROUP_HDR H " +
                         "                                   INNER JOIN SIIXSEM.DJ_GROUP_DTL D ON H.DJ_ID = D.DJ_ID " +
                         "                                   WHERE H.GROUP_NO IN( " +
                                                             DJS +
                         "                                   ) " +
                         "                                   AND D.STATUS = 'S' " +
                         "                                   AND H.GROUP_NO = D.GROUP_NO " +
                         "                                   AND D.COMPLETION_SUBINV IN('SMTB', 'SMTT', 'MANU', 'WIP', 'PACK', 'OQC') " +
                         "                                   ) " +
                         "               GROUP BY  pl.DJ_NO, pl.ITEM_CD, pl.ITEM_ID, OPEN_QTY " +
                         "               ORDER BY pl.DJ_NO, ITEM_CD) e " +
                        ") results " +
                        strFilter;

            try
            {
                // Create command.
                OracleCommand cmd = new OracleCommand();

                // Set connection for command.
                cmd.Connection = m_OracleDB;
                cmd.CommandText = query;


                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read()) { 
                            getAssyDJPicked_Result dj = new getAssyDJPicked_Result();
                            dj.MODEL_NAME = reader.GetString(0);
                            dj.DJ_STATUS = reader.GetString(1);
                            dj.SUBINV = reader.GetString(2);
                            dj.GROUP_NO = reader.GetInt32(3).ToString();
                            dj.DJ_NO = reader.GetString(4);
                            dj.SUBASSEMBLY = reader.GetString(5);
                            dj.ITEM_CD = reader.GetString(6);
                            dj.BALANCE = reader.GetInt32(7);
                            dj.CANTIDAD = reader.GetInt32(8);
                            dj.PICKED = reader.GetInt32(9);
                            dj.PORCENTAJE = reader.GetString(10);
                            dj.IWH_EXISTS_QTY = reader.GetInt32(11);
                            assyDjs.Add(dj);
                        }
                        result = true;
                    }
                    else result = false;
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex, "[getSimosOnHand] Error ");
                result = false;
            }
            return result;
        }

        public bool getAssyDJs(String DJS, ref List<getAssyDJPicked_Result> assyDjs, int filter)
        {
            assyDjs = new List<getAssyDJPicked_Result>();
            bool result = false;

            //String strFilter = "";

            //if (filter == 0)
            //    strFilter = "WHERE results.SUBASSEMBLY LIKE '%MANU%'";
            //else
            //    strFilter = "WHERE results.SUBASSEMBLY LIKE '%SMT%'";

            //String query = "SELECT h.group_no AS DJ_GROUP, d.assembly_desc AS MODEL_NAME, D.DJ_NO DJ_NUMBER, D.DJ_QTY AS QTY, H.FG_QTY FROM SIIXSEM.DJ_GROUP_HDR H " +
            //                "INNER JOIN SIIXSEM.DJ_GROUP_DTL D ON H.DJ_ID = D.DJ_ID " +
            //                "WHERE H.GROUP_NO IN(" + DJS + ") " +
            //                "AND D.STATUS = 'S' " +
            //                "AND H.GROUP_NO = D.GROUP_NO " +
            //                "AND D.ASSEMBLY_NAME LIKE '%MANU%'";

            String query = "select A.GROUP_NO, A.ASSEMBLY_DESC, A.DJ_NO AS MANU_DJ_NUMBER, A.DJ_QTY AS MANU_DJ_QTY, " +
                            "(SELECT FG.DJ_NO FROM siixsem.dj_group_dtl FG WHERE FG.group_no = A.group_no AND FG.completion_subinv = 'PACK') AS FG_DJ_NUMBER, " +
                            "(SELECT FG.DJ_QTY FROM siixsem.dj_group_dtl FG WHERE FG.group_no = A.group_no AND FG.completion_subinv = 'PACK' ) AS FG_QTY " +
                            "FROM siixsem.dj_group_dtl A "+
                            "where A.group_no IN(" + DJS +") AND(A.assembly_name LIKE '%MANU%')";
            try
            {
                // Create command.
                OracleCommand cmd = new OracleCommand();

                // Set connection for command.
                cmd.Connection = m_OracleDB;
                cmd.CommandText = query;


                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            getAssyDJPicked_Result dj = new getAssyDJPicked_Result();
                            dj.MODEL_NAME = reader.GetString(1); // nombre
                            dj.DJ_STATUS = "";
                            dj.SUBINV = reader.GetString(4); //// dj FG
                            dj.GROUP_NO = reader.GetInt32(0).ToString(); /// dj group
                            dj.DJ_NO = reader.GetString(2); /// dj manu
                            dj.SUBASSEMBLY = "";
                            dj.ITEM_CD = "";
                            dj.BALANCE = 0;
                            dj.CANTIDAD = reader.GetInt32(3); /// manu qty
                            dj.PICKED = reader.GetInt32(5); //// fg qty
                            dj.PORCENTAJE = "";
                            dj.IWH_EXISTS_QTY = 0;
                            assyDjs.Add(dj);
                        }
                        result = true;
                    }
                    else result = false;
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex, "[getSimosOnHand] Error ");
                result = false;
            }
            return result;
        }
        //@BOX_BARCODE NVARCHAR(150),
        //@BOX_NAME NVARCHAR(70),
        //@BOX_NUMBER AS NVARCHAR(10),
        //@BOX_QUANTITY AS Nvarchar(10),
        //@MODEL_NAME AS NVARCHAR(70),
        //@BOX_INTERNAL_PN AS NVARCHAR(50),
        //@BOX_CLIENT_PN AS NVARCHAR(50),
        //@BIN AS NVARCHAR(30),
        //@DATEE AS NVARCHAR(70),
        //@STPACK AS NVARCHAR(10)

        public bool insertBox(String BOX_BARCODE, String BOX_NAME, String BOX_NUMBER, String BOX_QUANTITY, String MODEL_NAME, String BOX_INTERNAL_PN, String BOX_CLIENT_PN, String BIN, String DATEE, String STPACK)
        {
            bool result = false;


            String query = "INSERT INTO SIIXSEM.PACKING_HDR (BOX_BARCODE, BOX_NAME, BOX_NUMBER, BOX_QUANTITY,MODEL_NAME,BOX_INTERNAL_PN,BOX_CLIENT_PN,	BIN, CREATED_DT,PRODUCTION_DT) VALUES('" + BOX_BARCODE + "', '" + BOX_NAME + "', " + BOX_NUMBER + ", " + BOX_QUANTITY + ", '" + MODEL_NAME + "', '" + BOX_INTERNAL_PN + "', '" + BOX_CLIENT_PN + "', '" + BIN + "',SYSDATE, '" + DATEE + "') ";

            try
            {
                // Create command.
                OracleCommand cmd = new OracleCommand();

                // Set connection for command.
                cmd.Connection = m_OracleDB;
                cmd.CommandText = query;


                cmd.ExecuteNonQuery();
                //query = "COMMIT";
                //cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                //logger.Error(ex, "[getSimosOnHand] Error ");
                result = false;
            }
            return result;

        }


        public void Close()
        {
            m_OracleDB.Dispose();
            m_OracleDB.Close();
            OracleConnection.ClearPool(m_OracleDB);
        }

    }
}