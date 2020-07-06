using AutoMapper;
using Domain;
using Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class CardService : GenericService, ICardService
    {
        private readonly IGenericService _service;

        public CardService(IUnitOfWork uow,
            IDateTimeService dateTimeService,
            IGenericService service,
            IPrincipalProvider principalProvider,
            ToggleOptions toggleOptions) : base(uow, dateTimeService, principalProvider, toggleOptions)
        {
            _service = service;
        }

        public async Task<PostCardResult> PostCardAsync(PostCardModel model)
        {
            try
            {
                var card = await _service.FirstOrDefaultAsync<Card>(c => c.CardCode == model.CardCode, null, true);

                if (card != null)
                {
                    return new PostCardResult(false, model.CardCode, $"There is an existing card with code {model.CardCode}");
                }

                await _service.CreateAsync(Mapper.Map<Card>(model));

                return new PostCardResult(true, model.CardCode);
            }
            catch (Exception ex)
            {
                return new PostCardResult(false, model.CardCode, ex.InnermostMsg());
            }
        }
    }

    public interface ICardService
    {
        Task<PostCardResult> PostCardAsync(PostCardModel model);
    }

    public class PostCardResult
    {
        public PostCardResult(bool succeed, string cardCode, string error = null)
        {
            Succeed = succeed;
            Error = error;
            CardCode = cardCode;
        }

        public bool Succeed { get; set; }
        public string Error { get; set; }
        public string CardCode { get; set; }
    }
}
