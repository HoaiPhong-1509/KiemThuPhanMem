/*
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
var showControllersOnly = false;
var seriesFilter = "";
var filtersOnlySampleSeries = true;

/*
 * Add header in statistics table to group metrics by category
 * format
 *
 */
function summaryTableHeader(header) {
    var newRow = header.insertRow(-1);
    newRow.className = "tablesorter-no-sort";
    var cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Requests";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 3;
    cell.innerHTML = "Executions";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 7;
    cell.innerHTML = "Response Times (ms)";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Throughput";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 2;
    cell.innerHTML = "Network (KB/sec)";
    newRow.appendChild(cell);
}

/*
 * Populates the table identified by id parameter with the specified data and
 * format
 *
 */
function createTable(table, info, formatter, defaultSorts, seriesIndex, headerCreator) {
    var tableRef = table[0];

    // Create header and populate it with data.titles array
    var header = tableRef.createTHead();

    // Call callback is available
    if(headerCreator) {
        headerCreator(header);
    }

    var newRow = header.insertRow(-1);
    for (var index = 0; index < info.titles.length; index++) {
        var cell = document.createElement('th');
        cell.innerHTML = info.titles[index];
        newRow.appendChild(cell);
    }

    var tBody;

    // Create overall body if defined
    if(info.overall){
        tBody = document.createElement('tbody');
        tBody.className = "tablesorter-no-sort";
        tableRef.appendChild(tBody);
        var newRow = tBody.insertRow(-1);
        var data = info.overall.data;
        for(var index=0;index < data.length; index++){
            var cell = newRow.insertCell(-1);
            cell.innerHTML = formatter ? formatter(index, data[index]): data[index];
        }
    }

    // Create regular body
    tBody = document.createElement('tbody');
    tableRef.appendChild(tBody);

    var regexp;
    if(seriesFilter) {
        regexp = new RegExp(seriesFilter, 'i');
    }
    // Populate body with data.items array
    for(var index=0; index < info.items.length; index++){
        var item = info.items[index];
        if((!regexp || filtersOnlySampleSeries && !info.supportsControllersDiscrimination || regexp.test(item.data[seriesIndex]))
                &&
                (!showControllersOnly || !info.supportsControllersDiscrimination || item.isController)){
            if(item.data.length > 0) {
                var newRow = tBody.insertRow(-1);
                for(var col=0; col < item.data.length; col++){
                    var cell = newRow.insertCell(-1);
                    cell.innerHTML = formatter ? formatter(col, item.data[col]) : item.data[col];
                }
            }
        }
    }

    // Add support of columns sort
    table.tablesorter({sortList : defaultSorts});
}

$(document).ready(function() {

    // Customize table sorter default options
    $.extend( $.tablesorter.defaults, {
        theme: 'blue',
        cssInfoBlock: "tablesorter-no-sort",
        widthFixed: true,
        widgets: ['zebra']
    });

    var data = {"OkPercent": 65.9167199488491, "KoPercent": 34.08328005115089};
    var dataset = [
        {
            "label" : "FAIL",
            "data" : data.KoPercent,
            "color" : "#FF6347"
        },
        {
            "label" : "PASS",
            "data" : data.OkPercent,
            "color" : "#9ACD32"
        }];
    $.plot($("#flot-requests-summary"), dataset, {
        series : {
            pie : {
                show : true,
                radius : 1,
                label : {
                    show : true,
                    radius : 3 / 4,
                    formatter : function(label, series) {
                        return '<div style="font-size:8pt;text-align:center;padding:2px;color:white;">'
                            + label
                            + '<br/>'
                            + Math.round10(series.percent, -2)
                            + '%</div>';
                    },
                    background : {
                        opacity : 0.5,
                        color : '#000'
                    }
                }
            }
        },
        legend : {
            show : true
        }
    });

    // Creates APDEX table
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.0612412084398977, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.0293125, 500, 1500, "GET Zing News Home - 1000"], "isController": false}, {"data": [0.0685, 500, 1500, "GET Zing News Home"], "isController": false}, {"data": [0.0014375, 500, 1500, "GET VnExpress Home - 1000"], "isController": false}, {"data": [0.10624829839368363, 500, 1500, "GET Zing News Home - 1000-0"], "isController": false}, {"data": [0.10634886703719539, 500, 1500, "GET Zing News Home-1"], "isController": false}, {"data": [0.055132044650149745, 500, 1500, "GET Zing News Home - 1000-1"], "isController": false}, {"data": [0.16695168875587857, 500, 1500, "GET Zing News Home-0"], "isController": false}, {"data": [0.0025, 500, 1500, "GET VnExpress Home"], "isController": false}]}, function(index, item){
        switch(index){
            case 0:
                item = item.toFixed(3);
                break;
            case 1:
            case 2:
                item = formatDuration(item);
                break;
        }
        return item;
    }, [[0, 0]], 3);

    // Create statistics table
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 50048, 17058, 34.08328005115089, 17635.482077205845, 3, 150368, 10003.0, 38915.000000000015, 51521.45000000001, 74960.32000000011, 96.89647828696444, 4537.710534479608, 9.510029603734681], "isController": false}, "titles": ["Label", "#Samples", "FAIL", "Error %", "Average", "Min", "Max", "Median", "90th pct", "95th pct", "99th pct", "Transactions/s", "Received", "Sent"], "items": [{"data": ["GET Zing News Home - 1000", 8000, 2076, 25.95, 29378.05225000004, 21, 150368, 22395.0, 63877.90000000003, 75363.95, 97048.9799999999, 15.910645813113554, 1160.6290556609083, 2.877857483621979], "isController": false}, {"data": ["GET Zing News Home", 5000, 1064, 21.28, 26215.55839999996, 24, 125769, 17973.5, 62046.900000000016, 72492.24999999999, 92316.10999999996, 9.703989504164953, 750.3057230521425, 1.8224812506792794], "isController": false}, {"data": ["GET VnExpress Home - 1000", 8000, 7295, 91.1875, 12252.824875000046, 590, 89477, 10008.0, 11915.400000000014, 29586.249999999996, 60536.85, 15.978748264807805, 462.6457963252624, 0.15813928412711092], "isController": false}, {"data": ["GET Zing News Home - 1000-0", 7346, 0, 0.0, 6792.769398312017, 16, 37868, 4903.5, 16427.7, 19227.949999999997, 26165.649999999998, 14.62580361284825, 5.9702987404009455, 1.6139802814959494], "isController": false}, {"data": ["GET Zing News Home-1", 4678, 742, 15.861479264643009, 21317.711415134643, 3, 113349, 13430.0, 51605.10000000002, 62086.30000000002, 80247.71, 9.088171583436138, 745.4108079430434, 0.8214178169349277], "isController": false}, {"data": ["GET Zing News Home - 1000-1", 7346, 1422, 19.357473454941466, 24209.707732099127, 31, 133316, 18639.0, 53461.2, 64785.3, 86037.2499999999, 14.632445945003834, 1152.4831786192694, 1.267575343352555], "isController": false}, {"data": ["GET Zing News Home-0", 4678, 0, 0.0, 5924.822573749472, 14, 37705, 4061.0, 14737.800000000003, 18324.45, 25076.740000000005, 9.081361466529223, 3.707040129891812, 1.0021424274587913], "isController": false}, {"data": ["GET VnExpress Home", 5000, 4459, 89.18, 12662.205399999983, 514, 92618, 10008.0, 14528.600000000002, 35832.59999999995, 60721.93, 9.683073020053644, 338.497095531988, 0.11766257577004638], "isController": false}]}, function(index, item){
        switch(index){
            // Errors pct
            case 3:
                item = item.toFixed(2) + '%';
                break;
            // Mean
            case 4:
            // Mean
            case 7:
            // Median
            case 8:
            // Percentile 1
            case 9:
            // Percentile 2
            case 10:
            // Percentile 3
            case 11:
            // Throughput
            case 12:
            // Kbytes/s
            case 13:
            // Sent Kbytes/s
                item = item.toFixed(2);
                break;
        }
        return item;
    }, [[0, 0]], 0, summaryTableHeader);

    // Create error table
    createTable($("#errorsTable"), {"supportsControllersDiscrimination": false, "titles": ["Type of error", "Number of errors", "% in errors", "% in all samples"], "items": [{"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to zingnews.vn:443 [zingnews.vn/42.112.59.9] failed: Read timed out", 443, 2.5970219251963886, 0.8851502557544757], "isController": false}, {"data": ["Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 1427, 8.365576269199202, 2.851262787723785], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: zingnews.vn:443 failed to respond", 190, 1.113846875366397, 0.3796355498721228], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 11254, 65.97490913354437, 22.48641304347826], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to znews.vn:443 [znews.vn/42.112.59.10, znews.vn/42.112.59.12] failed: Read timed out", 12, 0.07034822370735139, 0.0239769820971867], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to znews.vn:443 [znews.vn/42.112.59.12, znews.vn/42.112.59.10] failed: Read timed out", 48, 0.28139289482940555, 0.0959079283887468], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Read timed out", 108, 0.6331340133661625, 0.21579283887468031], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: znews.vn:443 failed to respond", 254, 1.4890374018056045, 0.5075127877237852], "isController": false}, {"data": ["Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 3322, 19.474733262985108, 6.637627877237851], "isController": false}]}, function(index, item){
        switch(index){
            case 2:
            case 3:
                item = item.toFixed(2) + '%';
                break;
        }
        return item;
    }, [[1, 1]]);

        // Create top5 errors by sampler
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 50048, 17058, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 11254, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 3322, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 1427, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to zingnews.vn:443 [zingnews.vn/42.112.59.9] failed: Read timed out", 443, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: znews.vn:443 failed to respond", 254], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": ["GET Zing News Home - 1000", 8000, 2076, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 1001, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 544, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to zingnews.vn:443 [zingnews.vn/42.112.59.9] failed: Read timed out", 292, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: zingnews.vn:443 failed to respond", 138, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: znews.vn:443 failed to respond", 81], "isController": false}, {"data": ["GET Zing News Home", 5000, 1064, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 528, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 277, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to zingnews.vn:443 [zingnews.vn/42.112.59.9] failed: Read timed out", 151, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: zingnews.vn:443 failed to respond", 52, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: znews.vn:443 failed to respond", 46], "isController": false}, {"data": ["GET VnExpress Home - 1000", 8000, 7295, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 7004, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 228, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Read timed out", 63, "", "", "", ""], "isController": false}, {"data": [], "isController": false}, {"data": ["GET Zing News Home-1", 4678, 742, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 489, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 197, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: znews.vn:443 failed to respond", 46, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to znews.vn:443 [znews.vn/42.112.59.12, znews.vn/42.112.59.10] failed: Read timed out", 8, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to znews.vn:443 [znews.vn/42.112.59.10, znews.vn/42.112.59.12] failed: Read timed out", 2], "isController": false}, {"data": ["GET Zing News Home - 1000-1", 7346, 1422, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 912, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 409, "Non HTTP response code: org.apache.http.NoHttpResponseException/Non HTTP response message: znews.vn:443 failed to respond", 81, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to znews.vn:443 [znews.vn/42.112.59.12, znews.vn/42.112.59.10] failed: Read timed out", 16, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to znews.vn:443 [znews.vn/42.112.59.10, znews.vn/42.112.59.12] failed: Read timed out", 4], "isController": false}, {"data": [], "isController": false}, {"data": ["GET VnExpress Home", 5000, 4459, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 4250, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 164, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Read timed out", 45, "", "", "", ""], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
