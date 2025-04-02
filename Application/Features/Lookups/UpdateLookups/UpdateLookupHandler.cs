using Ardalis.Result;
using Infrastructure.Context;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;
using Domain.Entities;

namespace Application.Features.Lookups.UpdateLookups;
public class UpdateLookupHandler : IRequestHandler<UpdateLookupRequest, Result<bool>>
{
    private readonly IContext _context;
    public UpdateLookupHandler(IContext context)
    {
        _context = context;
    }
    public async Task<Result<bool>> Handle(UpdateLookupRequest request, CancellationToken cancellationToken)
    {
            // Use FirstOrDefaultAsync instead of FirstOrDefault for async operations
            var lookup = await _context.Lookups.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // Check if lookup exists
            if (lookup == null)
            {
                return Result.Invalid(new List<ValidationError>
            {
                new ValidationError
                {
                    Identifier = "Not found",
                    ErrorMessage = "Lookup not found"
                }
            });
            }

            lookup.Name = request.Name;

            // Update() is not necessary when you're modifying a tracked entity
            // _context.Lookups.Update(lookup);  // This line is not needed

            // Await the SaveChangesAsync and pass the cancellationToken
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        
    }
}