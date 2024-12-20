using getAssyDJ.Models;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace getAssyDJ.Controllers
{
    /// <summary>
    /// Descripción breve de getAssyDJs
    /// </summary>
    public class getAssyDJs : IHttpHandler
    {
        COracle m_oracle = new COracle();
        public void ProcessRequest(HttpContext context)
        {
            String json = "";
            try
            {
                String tablehtml = "";
                String dj_groups = context.Request.Form["dj_groups"];
                String filter = context.Request.Form["filter"];
                List<getAssyDJPicked_Result> assyDjs = new List<getAssyDJPicked_Result>();
                json = "{";

                var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);

                if (filter.Contains("1")) //////// SUBASSY's SMT
                {
                    if (m_oracle.getSMTDJs(dj_groups, ref assyDjs, Convert.ToInt32(filter)))
                    {
                        tablehtml = "<table id='tblDJs' class='table table-striped table-bordered display nowrap' style='width:100%; font-size:10px;color:black;'>";
                        tablehtml += "<thead>";
                        tablehtml += "<tr style='background-color:blue;color:white;'>";
                        tablehtml += "<th>DJ GROUP</th>" +
                            "<th>DJ NUMBER</th>" +
                            "<th>QR</th>" +
                            "<th>MODEL</th>" +
                            "<th>SUBASSEMBLY</th>" +
                            "<th>SUBINV</th>" +
                            "<th>ITEM</th>" +
                            "<th>QTY</th>" +
                            "<th>PICKED QTY</th>" +
                            "<th>BALANCE</th>" +
                            "<th>PICKED %</th>" +
                            "<th>STATUS</th>" +
                            "<th>IWH QTY</th>";
                        tablehtml += "</tr>";
                        tablehtml += "</thead>";
                        tablehtml += "<tbody>";

                        foreach (getAssyDJPicked_Result dj in assyDjs)
                        {

                            var qrCode = qrEncoder.Encode(dj.DJ_NO);
                            string path = context.Server.MapPath("~/images/qr") + "\\" + dj.DJ_NO + ".png";
                            path.Replace("\\", "\\\\");
                            var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);
                            using (var stream = new FileStream(path, FileMode.Create))
                                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);

                            if (dj.PICKED == 0)
                                tablehtml += "<tr style='color:red;font-weight:bold;'>";
                            else
                                if ((dj.PICKED - dj.CANTIDAD) > 0)
                                tablehtml += "<tr style='color:black;font-weight:bold;'>";
                            else
                                tablehtml += "<tr>";
                            tablehtml += "<td>" + dj.GROUP_NO + "</td>";
                            tablehtml += "<td>" + dj.DJ_NO + "</td>";
                            tablehtml += "<td>" + "<img id ='" + dj.DJ_NO + "' src='" + "/getdjs/images/qr/" + dj.DJ_NO + ".png" + "' style='width:80%;'/>" + "</td>";
                            tablehtml += "<td>" + dj.MODEL_NAME + "</td>";
                            tablehtml += "<td>" + dj.SUBASSEMBLY + "</td>";
                            tablehtml += "<td>" + dj.SUBINV + "</td>";
                            tablehtml += "<td>" + dj.ITEM_CD + "</td>";
                            tablehtml += "<td>" + dj.CANTIDAD + "</td>";
                            tablehtml += "<td>" + dj.PICKED + "</td>";
                            tablehtml += "<td>" + dj.BALANCE + "</td>";
                            tablehtml += "<td>" + dj.PORCENTAJE + "</td>";
                            tablehtml += "<td>" + dj.DJ_STATUS + "</td>";
                            tablehtml += "<td>" + dj.IWH_EXISTS_QTY + "</td>";
                            tablehtml += "</tr>";
                        }
                        tablehtml += "</tbody>";
                        tablehtml += "</table>";
                        json += "\"result\":\"true\",";
                        json += "\"html\":\"" + tablehtml + "\"";

                    }
                    else
                    {
                        json += "\"result\":\"false\",";
                        json += "\"html\":\"" + "Ocurrio un error al obtener las DJs." + "\"";
                    }
                }
                else
                {
                    if (m_oracle.getAssyDJs(dj_groups, ref assyDjs, Convert.ToInt32(filter)))
                    {
                        tablehtml = "<table id='tblDJs' class='table table-striped table-bordered display nowrap' style='width:70%; font-size:12px;color:black;margin-right:auto;margin-left:auto;'>";
                        tablehtml += "<thead>";
                        tablehtml += "<tr style='background-color:blue;color:white;'>";
                        tablehtml += "<th>DJ GROUP</th>" +
                            "<th>MODEL</th>" +
                            "<th>MANU DJ NUMBER</th>" +
                            "<th>MANU QR</th>" +
                            "<th>MANU QTY</th>" +
                            "<th>FG DJ NUMBER</th>" +
                            "<th>FG QR</th>" +
                            "<th>FG QTY</th>";
                        tablehtml += "</tr>";
                        tablehtml += "</thead>";
                        tablehtml += "<tbody>";

                        foreach (getAssyDJPicked_Result dj in assyDjs)
                        {
                            getQr(context, qrEncoder, dj.DJ_NO);
                            getQr(context, qrEncoder, dj.SUBINV);

                            if (dj.PICKED == 0)
                                tablehtml += "<tr style='color:red;font-weight:bold;'>";
                            else
                                if ((dj.PICKED - dj.CANTIDAD) > 0)
                                tablehtml += "<tr style='color:black;font-weight:bold;'>";
                            else
                                tablehtml += "<tr>";
                            tablehtml += "<td>" + dj.GROUP_NO + "</td>";
                            tablehtml += "<td>" + dj.MODEL_NAME + "</td>";
                            tablehtml += "<td>" + dj.DJ_NO + "</td>";
                            tablehtml += "<td>" + "<img id ='" + dj.DJ_NO + "' src='" + "/getdjs/images/qr/" + dj.DJ_NO + ".png" + "' style='width:60%;'/>" + "</td>";
                            tablehtml += "<td>" + dj.CANTIDAD + "</td>";
                            tablehtml += "<td>" + dj.SUBINV + "</td>";
                            tablehtml += "<td>" + "<img id ='" + dj.SUBINV + "' src='" + "/getdjs/images/qr/" + dj.SUBINV + ".png" + "' style='width:60%;'/>" + "</td>";
                            tablehtml += "<td>" + dj.PICKED + "</td>";
                            tablehtml += "</tr>";
                        }
                        tablehtml += "</tbody>";
                        tablehtml += "</table>";
                        json += "\"result\":\"true\",";
                        json += "\"html\":\"" + tablehtml + "\"";

                    }
                    else
                    {
                        json += "\"result\":\"false\",";
                        json += "\"html\":\"" + "Ocurrio un error al obtener las DJs." + "\"";
                    }
                }
            }
            catch(Exception ex)
            {
                json += "\"result\":\"false\",";
                json += "\"html\":\"" + "Ocurrio una Excepción al obtener las DJs." + ex.Message.Replace("\"","'") +  "\"";
            }

            json += "}";
            context.Response.ContentType = "text/plain";
            context.Response.Write(json);
        }

        private static void getQr(HttpContext context, QrEncoder qrEncoder, /*getAssyDJPicked_Result dj*/ String param)
        {
            var qrCode = qrEncoder.Encode(param);
            string path = context.Server.MapPath("~/images/qr") + "\\" + param + ".png";
            path.Replace("\\", "\\\\");
            var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), Brushes.Black, Brushes.White);
            using (var stream = new FileStream(path, FileMode.Create))
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}