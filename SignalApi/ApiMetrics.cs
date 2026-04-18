using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace SignalApi
{
    public class ApiMetrics
    {
        private readonly Counter<long> _requestCounter;
        private readonly Histogram<double> _responseTimeHistogram;

        public ApiMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("crawlsoft.SignalApi");

            _requestCounter = meter.CreateCounter<long>("api.audio.requests_total", 
                description: "Total number of audio requests");

            _responseTimeHistogram = meter.CreateHistogram<double>("api.audio.response_time",
                unit: "ms", description: "Response time for audio requests");
        }

        public void RecordRequest(string method, string endpoint)
        {
            _requestCounter.Add(1, new TagList { { "method", method }, { "endpoint", endpoint }});
        }

        public void RecordResponseTime(double elapsedMs, string status)
        {
            _responseTimeHistogram.Record(elapsedMs, new TagList { { "status", status }});
        }
    }
}
