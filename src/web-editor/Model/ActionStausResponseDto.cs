using System;

namespace CollabEdit.Model
{
    public class ActionStausResponseDto<T>
    {
        public ActionStausResponseDto() { }
        public ActionStausResponseDto(T entity, ActionStatusResult result = ActionStatusResult.Ok, Exception ex = null)
        {
            Entity = entity;
            Status = result;
            if (ex != null)
                Errors = new string[] { ex.Message };
        }
        public T Entity { get; set; }
        public ActionStatusResult Status { get; set; }
        public string[] Errors { get; set; }
    }

    public enum ActionStatusResult
    {
        Ok = 200,
        ClientFauilure = 400,
        ServerFailure = 500
    }
}