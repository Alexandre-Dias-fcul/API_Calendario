using Assembly.Projecto.Final.Domain.Common;
using Assembly.Projecto.Final.Domain.Enums;
using Assembly.Projecto.Final.Domain.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Domain.Models
{
    public class Appointment:AuditableEntity<int>
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime Date { get; private set; }
        public TimeOnly HourStart { get; private set; }
        public TimeOnly HourEnd { get; private set; }
        public StatusType Status { get; private set; }

        private List<Participant> _participants;
        public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();

        private Appointment()
        {
            Title = string.Empty;
            Description = string.Empty;
            Date = DateTime.MinValue;
            HourStart = new TimeOnly();
            HourEnd = new TimeOnly();
            Status = 0;
            _participants = new (); 
        }

        private Appointment(string title,string description,DateTime date,TimeOnly hourStart,TimeOnly hourEnd,
            StatusType status):this() 
        {
            DomainValidation(title, description, date, hourStart, hourEnd, status);
        }

        private Appointment(int id,string title, string description, DateTime date, TimeOnly hourStart,TimeOnly hourEnd,
           StatusType status) : this(title, description,date,hourStart,hourEnd,status)
        {
            Id = Id;
        }

        public static Appointment Create(string title, string description, DateTime date, TimeOnly hourStart,
            TimeOnly hourEnd, StatusType status) 
        {
            var appointment = new Appointment(title, description,date,hourStart,hourEnd,status);

            return appointment;
        }

        public static Appointment Create(int id,string title, string description, DateTime date, 
            TimeOnly hourStart,TimeOnly hourEnd, StatusType status)
        {
            var appointment = new Appointment(id, title, description, date, hourStart, hourEnd,status);

            return appointment;
        }

        public void Update(string title, string description, DateTime date, TimeOnly hourStart, TimeOnly hourEnd,
            StatusType status)
        {
            DomainValidation(title, description,date,hourStart,hourEnd,status);
        }

        public void DomainValidation(string title, string description, DateTime date, TimeOnly hourStart, TimeOnly hourEnd,
            StatusType status) 
        {
            DomainExceptionValidation.When(title == null,"Erro: o título é obrigatório.");
            DomainExceptionValidation.When(title != null && title.Length>200, "Erro: o título não pode ter mais " +
                "de 200 caracteres.");
            DomainExceptionValidation.When(description == null,"Erro: a descrição é obrigatória.");
            DomainExceptionValidation.When(description != null && description.Length > 2000, " Erro: a descrição não " +
                "pode ter mais de 2000 caracteres.");

            DomainExceptionValidation.When(date < DateTime.Today, "Erro: a data tem de ser posterior à data atual.");
            DomainExceptionValidation.When(hourStart > hourEnd, "Erro: a hora de inicio não pode ser posterior à hora " +
                "de fim.");

            Title = title;
            Description = description;
            Date = date;
            HourStart = hourStart;
            HourEnd = hourEnd;
            Status = status;
        }

        public void AddParticipant(Participant participant) 
        {
            DomainExceptionValidation.When(participant == null, 
                $"Erro: Não foi encontrada a entidade {nameof(participant)}.");

            _participants.Add(participant);
        }
    }
}
