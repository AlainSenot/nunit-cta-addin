using NUnit.Core;
using NUnit.Core.Builders;
using NUnit.Core.Extensibility;
using System;
using System.Collections;
using System.Reflection;

namespace CTA.NUnitAddin
{
    public class CTATestCaseBuilder : NUnitTestCaseBuilder, ITestCaseBuilder2
    {
        private ScreenCapture screenCapture;
        private IExtensionHost Host;

        public CTATestCaseBuilder(IExtensionHost host, ScreenCapture screenCap)
        {
            this.Host = host;
            this.screenCapture = screenCap;
        }

        #region ITestCaseBuilder Methods
        public bool CanBuildFrom(MethodInfo method)
        {
            return NUnit.Core.Reflect.HasAttribute(method, "NUnit.Framework.TestAttribute", false)
                || NUnit.Core.Reflect.HasAttribute(method, "NUnit.Framework.TestCaseAttribute", false)
                || NUnit.Core.Reflect.HasAttribute(method, "NUnit.Framework.TestCaseSourceAttribute", false)
                || NUnit.Core.Reflect.HasAttribute(method, "NUnit.Framework.TheoryAttribute", false);
        }

        public Test BuildFrom(MethodInfo method)
        {
            return BuildFrom(method, null);
        }

        #region ITestCaseBuilder2 Members
        public bool CanBuildFrom(MethodInfo method, Test parentSuite)
        {
            return CanBuildFrom(method);
        }

        public Test BuildFrom(MethodInfo method, Test parentSuite)
        {

            ITestCaseProvider testCaseProvider = Host.GetExtensionPoint("TestCaseProviders") as ITestCaseProvider;

            return testCaseProvider.HasTestCasesFor(method)
                ? BuildParameterizedMethodSuite(method, parentSuite)
                : BuildSingleTestMethod(method, parentSuite, null, this.screenCapture, null);
        }

        #endregion

        public Test BuildParameterizedMethodSuite(MethodInfo method, Test parentSuite)
        {
            ParameterizedMethodSuite methodSuite = new ParameterizedMethodSuite(method);
            NUnitFramework.ApplyCommonAttributes(method, methodSuite);

            if (parentSuite != null)
            {
                if (parentSuite.RunState == RunState.NotRunnable && methodSuite.RunState != RunState.NotRunnable)
                {
                    methodSuite.RunState = RunState.NotRunnable;
                    methodSuite.IgnoreReason = parentSuite.IgnoreReason;
                }

                if (parentSuite.RunState == RunState.Ignored && methodSuite.RunState != RunState.Ignored && methodSuite.RunState != RunState.NotRunnable)
                {
                    methodSuite.RunState = RunState.Ignored;
                    methodSuite.IgnoreReason = parentSuite.IgnoreReason;
                }
            }

            ITestCaseProvider2 testCaseProvider = Host.GetExtensionPoint("TestCaseProviders") as ITestCaseProvider2;

            foreach (object source in testCaseProvider.GetTestCasesFor(method, parentSuite))
            {
                ParameterSet parms;

                if (source == null)
                {
                    parms = new ParameterSet();
                    parms.Arguments = new object[] { null };
                }
                else
                    parms = source as ParameterSet;

                if (parms == null)
                {
                    if (source.GetType().GetInterface("NUnit.Framework.ITestCaseData") != null)
                        parms = ParameterSet.FromDataSource(source);
                    else
                    {
                        parms = new ParameterSet();

                        ParameterInfo[] parameters = method.GetParameters();
                        Type sourceType = source.GetType();

                        if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(sourceType))
                            parms.Arguments = new object[] { source };
                        else if (source is object[])
                            parms.Arguments = (object[])source;
                        else if (source is Array)
                        {
                            Array array = (Array)source;
                            if (array.Rank == 1)
                            {
                                parms.Arguments = new object[array.Length];
                                for (int i = 0; i < array.Length; i++)
                                    parms.Arguments[i] = (object)array.GetValue(i);
                            }
                        }
                        else
                            parms.Arguments = new object[] { source };
                    }
                }

                TestMethod test = BuildSingleTestMethod(method, parentSuite, parms, this.screenCapture, methodSuite.Properties);

                methodSuite.Add(test);
            }

            return methodSuite;
        }

        public TestMethodExtension BuildSingleTestMethod(MethodInfo method, Test parentSuite, ParameterSet parms, ScreenCapture screenCapture, IDictionary properties)
        {
            //TODO : Here it doesn't support  Async Await Method, BTW, the Async Await just be supported from .net 4.5
            TestMethodExtension testMethod = new TestMethodExtension(method, screenCapture);

            string prefix = method.ReflectedType.FullName;

            if (parentSuite != null)
            {
                testMethod.Properties = properties;
                prefix = parentSuite.TestName.FullName;
                testMethod.TestName.FullName = prefix + "." + testMethod.TestName.Name;
            }

            if (CheckTestMethodSignature(testMethod, parms))
            {
                if (parms == null)
                    NUnitFramework.ApplyCommonAttributes(method, testMethod);
                NUnitFramework.ApplyExpectedExceptionAttribute(method, testMethod);
            }

            if (parms != null)
            {
                method = testMethod.Method;

                if (parms.TestName != null)
                {
                    testMethod.TestName.Name = parms.TestName;
                    testMethod.TestName.FullName = prefix + "." + parms.TestName;
                }
                else if (parms.OriginalArguments != null)
                {
                    string name = MethodHelper.GetDisplayName(method, parms.OriginalArguments);
                    testMethod.TestName.Name = name;
                    testMethod.TestName.FullName = prefix + "." + name;
                }

                if (parms.Ignored)
                {
                    testMethod.RunState = RunState.Ignored;
                    testMethod.IgnoreReason = parms.IgnoreReason;
                }
                else if (parms.Explicit)
                {
                    testMethod.RunState = RunState.Explicit;
                }

                foreach (string key in parms.Properties.Keys)
                    testMethod.Properties[key] = parms.Properties[key];

                if (parms.Description != null)
                    testMethod.Description = parms.Description;
            }

            if (parentSuite != null)
            {
                if (parentSuite.RunState == RunState.NotRunnable && testMethod.RunState != RunState.NotRunnable)
                {
                    testMethod.RunState = RunState.NotRunnable;
                    testMethod.IgnoreReason = parentSuite.IgnoreReason;
                }

                if (parentSuite.RunState == RunState.Ignored && testMethod.RunState != RunState.Ignored && testMethod.RunState != RunState.NotRunnable)
                {
                    testMethod.RunState = RunState.Ignored;
                    testMethod.IgnoreReason = parentSuite.IgnoreReason;
                }
            }

            return testMethod;
        }
        #endregion


        private static bool CheckTestMethodSignature(TestMethodExtension testMethod, ParameterSet parms)
        {
            if (testMethod.Method.IsAbstract)
            {
                testMethod.RunState = RunState.NotRunnable;
                testMethod.IgnoreReason = "Method is abstract";
                return false;
            }

            if (!testMethod.Method.IsPublic)
            {
                testMethod.RunState = RunState.NotRunnable;
                testMethod.IgnoreReason = "Method is not public";
                return false;
            }

            ParameterInfo[] parameters = testMethod.Method.GetParameters();
            int argsNeeded = parameters.Length;

            object[] arglist = null;
            int argsProvided = 0;

            if (parms != null)
            {
                testMethod.arguments = parms.Arguments;
                testMethod.hasExpectedResult = parms.HasExpectedResult;
                if (testMethod.hasExpectedResult)
                    testMethod.expectedResult = parms.Result;
                testMethod.RunState = parms.RunState;
                testMethod.IgnoreReason = parms.IgnoreReason;
                testMethod.BuilderException = parms.ProviderException;

                arglist = parms.Arguments;

                if (arglist != null)
                    argsProvided = arglist.Length;

                if (testMethod.RunState != RunState.Runnable)
                    return false;
            }

#if CLR_2_0 || CLR_4_0
	        bool isAsyncMethod = Reflect.IsAsyncMethod(testMethod.Method);
			bool hasMeaningfulReturnType = isAsyncMethod ? testMethod.Method.ReturnType.IsGenericType : testMethod.Method.ReturnType != typeof(void);
#else
            bool hasMeaningfulReturnType = testMethod.Method.ReturnType != typeof(void);
#endif

            if (hasMeaningfulReturnType && (parms == null || !parms.HasExpectedResult && parms.ExpectedExceptionName == null))
                return MarkAsNotRunnable(testMethod, "Test method has non-void return type, but no result is expected");
            if (!hasMeaningfulReturnType && parms != null && parms.HasExpectedResult)
                return MarkAsNotRunnable(testMethod, "Test method has void return type, but a result is expected");

            if (argsProvided > 0 && argsNeeded == 0)
                return MarkAsNotRunnable(testMethod, "Arguments provided for method not taking any");

            if (argsProvided == 0 && argsNeeded > 0)
                return MarkAsNotRunnable(testMethod, "No arguments were provided");

            if (argsProvided != argsNeeded)
            {
                testMethod.RunState = RunState.NotRunnable;
                testMethod.IgnoreReason = "Wrong number of arguments provided";
                return false;
            }

#if CLR_2_0 || CLR_4_0
            if (testMethod.Method.IsGenericMethodDefinition)
            {
                Type[] typeArguments = GetTypeArgumentsForMethod(testMethod.Method, arglist);
                foreach (object o in typeArguments)
                    if (o == null)
                    {
                        testMethod.RunState = RunState.NotRunnable;
                        testMethod.IgnoreReason = "Unable to determine type arguments for fixture";
                        return false;
                    }

                testMethod.method = testMethod.Method.MakeGenericMethod(typeArguments);
                parameters = testMethod.Method.GetParameters();

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (arglist[i].GetType() != parameters[i].ParameterType && arglist[i] is IConvertible)
                    {
                        try
                        {
                            arglist[i] = Convert.ChangeType(arglist[i], parameters[i].ParameterType);
                        }
                        catch (Exception)
                        {
                            // Do nothing - the incompatible argument will be reported below
                        }
                    }
                }
            }
#endif


            return true;
        }

        private static bool MarkAsNotRunnable(TestMethod testMethod, string reason)
        {
            testMethod.RunState = RunState.NotRunnable;
            testMethod.IgnoreReason = reason;
            return false;
        }

#if CLR_2_0 || CLR_4_0
        private static Type[] GetTypeArgumentsForMethod(MethodInfo method, object[] arglist)
        {
            Type[] typeParameters = method.GetGenericArguments();
            Type[] typeArguments = new Type[typeParameters.Length];
            ParameterInfo[] parameters = method.GetParameters();

            for (int typeIndex = 0; typeIndex < typeArguments.Length; typeIndex++)
            {
                Type typeParameter = typeParameters[typeIndex];

                for (int argIndex = 0; argIndex < parameters.Length; argIndex++)
                {
                    if (parameters[argIndex].ParameterType.Equals(typeParameter))
                        typeArguments[typeIndex] = TypeHelper.BestCommonType(
                            typeArguments[typeIndex],
                            arglist[argIndex].GetType());
                }
            }

            return typeArguments;
        }
#endif
    }
}
