using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prometheus.Models
{
    public class ATEUtility
    {
        public static void retrieveATEData()
        {
            var sql = "SELECT d.ROUTE_ID,a.MFR_SN,d.DATASET_NAME,c.FAMILY,d.STATUS,d.END_TIME,d.STATION,a.MFR_PN FROM PARTS a INNER JOIN ROUTES b ON a.OPT_INDEX = b.PART_INDEX INNER JOIN BOM_CONTEXT_ID c ON c.BOM_CONTEXT_ID = b.BOM_CONTEXT_ID INNER JOIN DATASETS d ON b.ROUTE_ID = d.ROUTE_ID WHERE(c.MODEL_ID like 'FTLX6871%' or c.MODEL_ID like 'FTLX6872%' or c.MODEL_ID like 'FTLX6672%') and d.END_TIME > '20161119080000'  and d.DATASET_NAME IN('final', 'assembly', 'tune_rf', 'tune', 'temp_test_7up') ORDER BY d.END_TIME DESC";
            DBUtility.ExeATESqlWithRes(sql,null);
        }

    }
}