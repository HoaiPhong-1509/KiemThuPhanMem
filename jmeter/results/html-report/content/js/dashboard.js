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

    var data = {"OkPercent": 75.97115384615384, "KoPercent": 24.028846153846153};
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
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.7520769230769231, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.9915625, 500, 1500, "GET Zing News Home - 1000"], "isController": false}, {"data": [0.9906, 500, 1500, "GET Zing News Home"], "isController": false}, {"data": [0.018125, 500, 1500, "GET VnExpress Home - 1000"], "isController": false}, {"data": [0.9996875, 500, 1500, "GET Zing News Home - 1000-0"], "isController": false}, {"data": [0.9957, 500, 1500, "GET Zing News Home-1"], "isController": false}, {"data": [0.9969375, 500, 1500, "GET Zing News Home - 1000-1"], "isController": false}, {"data": [0.9998, 500, 1500, "GET Zing News Home-0"], "isController": false}, {"data": [0.0254, 500, 1500, "GET VnExpress Home"], "isController": false}]}, function(index, item){
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
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 52000, 12495, 24.028846153846153, 2346.971057692297, 7, 30749, 123.0, 10004.0, 10010.0, 10015.0, 198.8923227563416, 10212.577816322462, 22.237052904640695], "isController": false}, "titles": ["Label", "#Samples", "FAIL", "Error %", "Average", "Min", "Max", "Median", "90th pct", "95th pct", "99th pct", "Transactions/s", "Received", "Sent"], "items": [{"data": ["GET Zing News Home - 1000", 8000, 0, 0.0, 176.1368749999997, 51, 10324, 145.0, 288.0, 361.0, 591.0, 30.739082822616282, 2937.2774831115157, 6.69415573187835], "isController": false}, {"data": ["GET Zing News Home", 5000, 0, 0.0, 172.15539999999953, 49, 1489, 140.0, 284.90000000000055, 360.0, 591.9799999999996, 22.66741015771984, 2165.956455090477, 4.936359829269067], "isController": false}, {"data": ["GET VnExpress Home - 1000", 8000, 7732, 96.65, 9202.656250000011, 12, 30749, 10001.0, 10012.0, 10014.0, 10041.0, 30.648757575989766, 388.49365052921786, 0.2981631756614385], "isController": false}, {"data": ["GET Zing News Home - 1000-0", 8000, 0, 0.0, 46.224625000000145, 7, 1105, 33.0, 95.0, 123.0, 206.98999999999978, 30.785333866945788, 12.566669488655606, 3.397209694301635], "isController": false}, {"data": ["GET Zing News Home-1", 5000, 0, 0.0, 127.40399999999997, 33, 1214, 108.0, 201.0, 252.0, 462.9899999999998, 22.677691048208235, 2157.6817285630623, 2.4360800930692443], "isController": false}, {"data": ["GET Zing News Home - 1000-1", 8000, 0, 0.0, 129.82325000000006, 37, 10265, 111.0, 199.0, 251.0, 436.9899999999998, 30.751725940618417, 2925.9326474437144, 3.3034080600273685], "isController": false}, {"data": ["GET Zing News Home-0", 5000, 0, 0.0, 44.64680000000005, 7, 1103, 30.0, 92.0, 126.0, 219.95999999999913, 22.67820533754241, 9.257314288176492, 2.5025753936936446], "isController": false}, {"data": ["GET VnExpress Home", 5000, 4763, 95.26, 8776.547200000005, 13, 30705, 10001.0, 10007.0, 10013.0, 10042.0, 22.558595952987886, 380.0341577677705, 0.33340018667238147], "isController": false}]}, function(index, item){
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
    createTable($("#errorsTable"), {"supportsControllersDiscrimination": false, "titles": ["Type of error", "Number of errors", "% in errors", "% in all samples"], "items": [{"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 11614, 92.94917967186875, 22.334615384615386], "isController": false}, {"data": ["429/Too Many Requests", 846, 6.770708283313326, 1.626923076923077], "isController": false}, {"data": ["Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Read timed out", 1, 0.008003201280512205, 0.0019230769230769232], "isController": false}, {"data": ["Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 34, 0.272108843537415, 0.06538461538461539], "isController": false}]}, function(index, item){
        switch(index){
            case 2:
            case 3:
                item = item.toFixed(2) + '%';
                break;
        }
        return item;
    }, [[1, 1]]);

        // Create top5 errors by sampler
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 52000, 12495, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 11614, "429/Too Many Requests", 846, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 34, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Read timed out", 1, "", ""], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": [], "isController": false}, {"data": [], "isController": false}, {"data": ["GET VnExpress Home - 1000", 8000, 7732, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 7288, "429/Too Many Requests", 425, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 19, "", "", "", ""], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": [], "isController": false}, {"data": ["GET VnExpress Home", 5000, 4763, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Connect timed out", 4326, "429/Too Many Requests", 421, "Non HTTP response code: java.net.SocketTimeoutException/Non HTTP response message: Read timed out", 15, "Non HTTP response code: org.apache.http.conn.ConnectTimeoutException/Non HTTP response message: Connect to vnexpress.net:443 [vnexpress.net/111.65.250.2] failed: Read timed out", 1, "", ""], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
