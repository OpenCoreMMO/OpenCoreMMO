using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

public class AccountPremiumHistoryRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger)
    : BaseRepository<AccountPremiumHistoryEntity>(contextOptions,
        logger), IAccountPremiumHistoryRepository;