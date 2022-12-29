using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaNatoIndexer
{
    internal class ParseMangaNato
    {
        private ILogger<ParseMangaNato> _logger;

        public ParseMangaNato(ILogger<ParseMangaNato> logger)
        {
            _logger = logger;
        }
        public async Task Index()
        {
            _logger.LogInformation("Init. Test.");
        }
    }
}
