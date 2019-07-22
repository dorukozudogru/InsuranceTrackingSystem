using System;

namespace SigortaTakipSistemi.Models.ViewModels
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public string RequestMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}