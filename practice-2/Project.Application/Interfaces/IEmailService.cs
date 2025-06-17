using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(Message message);

    }
}