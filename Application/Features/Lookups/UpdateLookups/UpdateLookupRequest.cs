﻿using Ardalis.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Lookups.UpdateLookups;
public class UpdateLookupRequest:IRequest<Result<bool>>
{
    public int Id { get; set; }
    public string Name { get; set; }
}
