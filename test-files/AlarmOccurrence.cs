using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using FG.Sitrad.Common.Enum;

namespace FG.Sitrad.Model
{
    [DataContract]
    [DebuggerDisplay("{AlarmCode}")]
    public class AlarmOccurrence : ICloneable
    {
        #region Propriedades adicionais

        /// <summary>
        /// Utilizado para verificar se é preciso salvar o objeto ao final do processamento de alarmes.
        /// </summary>
        [IgnoreDataMember]
        public bool IsSavePending { get; set; }

        [IgnoreDataMember]
        public AlarmInhibitionType PreviousInhibitionType = AlarmInhibitionType.Undefined;

        [IgnoreDataMember]
        public AlarmInhibitionType inhibitionType = AlarmInhibitionType.Undefined;

        #endregion



        #region Campos do banco de dados

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int InstrumentId { get; set; }

        [DataMember]
        public string AlarmCode { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember]
        public DateTime OccurrenceDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? OccurrenceFinalDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? AcknowledgeTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? AcknowledgeUserId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AcknowledgeUserName { get; set; }

        [DataMember]
        public AlarmInhibitionType InhibitionType
        {
            get => inhibitionType;
            set
            {
                PreviousInhibitionType = inhibitionType;
                inhibitionType = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public bool Inhibited { get; set; }

        [IgnoreDataMember]
        public DateTime? DelayEndDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? EventLogId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EventLog EventLog { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? AcknowledgeEventLogId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool OccurringOrUnacknowledged { get; private set; }

        [DataMember(EmitDefaultValue = false)]
        public EventLog AcknowledgeEventLog { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Instrument Instrument { get; set; }

        [IgnoreDataMember]
        public User AcknowledgeUser { get; set; }

        [DataMember]
        public IList<AlarmNotificationType> IgnoredTo { get; set; }

        #endregion



        #region IClonable

        public object Clone()
        {
            var occurrence = new AlarmOccurrence();
            CloneTo(ref occurrence);
            return occurrence;
        }

        public void CloneTo(ref AlarmOccurrence alarmOccurrence)
        {
            alarmOccurrence.Id = Id;
            alarmOccurrence.AlarmCode = AlarmCode;
            alarmOccurrence.Description = Description;
            alarmOccurrence.Value = Value;
            alarmOccurrence.OccurrenceDate = OccurrenceDate;
            alarmOccurrence.OccurrenceFinalDate = OccurrenceFinalDate;
            alarmOccurrence.AcknowledgeTime = AcknowledgeTime;
            alarmOccurrence.InhibitionType = InhibitionType;
            alarmOccurrence.Inhibited = Inhibited;
            alarmOccurrence.EventLog = EventLog;
            alarmOccurrence.EventLogId = EventLogId;
            alarmOccurrence.AcknowledgeEventLog = AcknowledgeEventLog;
            alarmOccurrence.AcknowledgeEventLogId = AcknowledgeEventLogId;
            alarmOccurrence.InstrumentId = InstrumentId;
            alarmOccurrence.Instrument = Instrument;
            alarmOccurrence.AcknowledgeUser = AcknowledgeUser;
            alarmOccurrence.AcknowledgeUserId = AcknowledgeUserId;
            alarmOccurrence.AcknowledgeUserName = AcknowledgeUserName;
            alarmOccurrence.IgnoredTo = IgnoredTo;
            alarmOccurrence.IsSavePending = IsSavePending;
        }

        #endregion
    }
}
