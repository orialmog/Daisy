//using Ancestry.Daisy;
//using Ancestry.Daisy.Program;

//namespace Thing
//{
//    public class Program
//    {
//        public static DaisyCompiledExecution Prg(Ancestry.Daisy.Tests.Daisy.Component.Domain.User scope, ContextBundle context)
//        {
//            var attachments = new ContextBundle();
//            var result = (new Ancestry.Daisy.Tests.Daisy.Component.Controllers.UserController()
//            {
//                Attachments = attachments,
//                Context = context,
//                Scope = scope
//            }.IsActive() && new Ancestry.Daisy.Tests.Daisy.Component.Controllers.UserController()
//            {
//                Attachments = attachments,
//                Context = context,
//                Scope = scope
//            }.HasAccount(i =>
//                new Ancestry.Daisy.Tests.Daisy.Component.Controllers.AccountController()
//                {
//                    Attachments = attachments,
//                    Context = context,
//                    Scope = i
//                }.BalanceBetween(100, 1000) || true));

//            return new DaisyCompiledExecution
//            {
//                Attachments = attachments,
//                Outcome = result
//            };
//        }
//    }
//}


//// initial scope
