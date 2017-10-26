function setupbeurschart(chartLabels, chartData) {
    var areaChartCanvas = $('#areaChart').get(0).getContext('2d');
    var areaChart = new Chart(areaChartCanvas,
        {
            type: 'line',
            data: {
                labels: chartLabels,
                datasets: [
                    {
                        label: 'Waarde',
                        backgroundColor: 'rgba(60,141,188,0.9)',
                        pointRadius: 0,
                        pointHitRadius: 10,
                        data: chartData
                    }
                ]
            },
            options: {
                responsive: true,
                legend: {
                    display: false
                },
                scales: {
                    xAxes: [{
                        ticks: {
                            autoSkip: true,
                            maxTicksLimit: 20
                        }
                    }]
                }

            }
        }
        );

}
