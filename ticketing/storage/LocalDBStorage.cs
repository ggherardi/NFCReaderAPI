using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public class LocalDBStorage : IValidationStorage
    {
        private string _connectionString = @"Data Source=USER-PC\SQLEXPRESS;Initial Catalog=NFCValidationStorage;Integrated Security=SSPI";

        public void RegisterValidation(ValidationEntity validation)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                sqlConnection.Open();
                string cardIDParameter = "@cardid";
                string locationParameter = "@location";
                string validationTimeParameter = "@validationTime";
                string encryptedTicketHashParameter = "@encryptedTicketHash";
                string commandString = $"INSERT INTO Validation (card_id, location, validation_time, encrypted_ticket) VALUES ({cardIDParameter}, {locationParameter}, {validationTimeParameter}, {encryptedTicketHashParameter})";
                SqlCommand command = new SqlCommand(commandString, sqlConnection);
                command.Parameters.AddWithValue(cardIDParameter, BitConverter.ToString(validation.CardId));
                command.Parameters.AddWithValue(locationParameter, validation.Location);
                command.Parameters.AddWithValue(validationTimeParameter, validation.Time);
                command.Parameters.AddWithValue(encryptedTicketHashParameter, validation.EncryptedTicketHash);
                command.ExecuteNonQuery();
                sqlConnection.Close();
            }
            catch(Exception ex) { }            
        }

        public void RegisterTicketUpdate(EncryptableSmartTicket ticket)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                sqlConnection.Open();                
                string creditParameter = "@credit";
                string ticketTypeParameter = "@ticketType";
                string currentValidationParameter = "@currentValidation";
                string sessionValidationParameter = "@sessionValidation";
                string sessionExpenseParameter = "@sessionExpense";
                string cardIDParameter = "@cardid";
                string commandString = $"UPDATE SmartTicket SET credit = {creditParameter}, ticket_type = {ticketTypeParameter}, current_validation = {currentValidationParameter}, session_validation = {sessionValidationParameter}, session_expense = {sessionExpenseParameter} WHERE card_id = {cardIDParameter}";
                SqlCommand command = new SqlCommand(commandString, sqlConnection);                
                command.Parameters.AddWithValue(creditParameter, ticket.Credit);
                command.Parameters.AddWithValue(ticketTypeParameter, ticket.TicketTypeName);
                command.Parameters.AddWithValue(currentValidationParameter, ticket.CurrentValidation);
                command.Parameters.AddWithValue(sessionValidationParameter, ticket.SessionValidation);
                command.Parameters.AddWithValue(sessionExpenseParameter, ticket.SessionExpense);
                command.Parameters.AddWithValue(cardIDParameter, BitConverter.ToString(ticket.CardID));
                command.ExecuteNonQuery();
                sqlConnection.Close();
            }
            catch (Exception ex) { }
        }

        public void RegisterTransaction(CreditTransaction creditTransaction)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                sqlConnection.Open();
                SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
                string cardIDParameter = "@cardid";
                string amountParameter = "@amount";
                string locationParameter = "@location";
                string dateParameter = "@date";
                string cardId = BitConverter.ToString(creditTransaction.CardId);
                string creditTransactionCommandString = $"INSERT INTO CreditTransaction (card_id, location, amount, date) VALUES ({cardIDParameter}, {locationParameter}, {amountParameter}, {dateParameter})";                
                SqlCommand creditTransactionCommand = new SqlCommand(creditTransactionCommandString, sqlConnection, sqlTransaction);
                creditTransactionCommand.Parameters.AddWithValue(cardIDParameter, cardId);
                creditTransactionCommand.Parameters.AddWithValue(locationParameter, creditTransaction.Location);
                creditTransactionCommand.Parameters.AddWithValue(amountParameter, creditTransaction.Amount);
                creditTransactionCommand.Parameters.AddWithValue(dateParameter, creditTransaction.Date);
                creditTransactionCommand.ExecuteNonQuery();
                
                string ticketCreditUpdateCommandString = $"UPDATE SmartTicket SET credit = credit + {amountParameter} WHERE card_id = {cardIDParameter}";
                SqlCommand ticketCreditUpdateCommand = new SqlCommand(ticketCreditUpdateCommandString, sqlConnection, sqlTransaction);
                ticketCreditUpdateCommand.Parameters.AddWithValue(amountParameter, creditTransaction.Amount);
                ticketCreditUpdateCommand.Parameters.AddWithValue(cardIDParameter, cardId);
                ticketCreditUpdateCommand.ExecuteNonQuery();

                sqlTransaction.Commit();
                sqlConnection.Close();
            }
            catch (Exception ex) { }
        }
    }
}
