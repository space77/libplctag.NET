﻿using KellermanSoftware.CompareNetObjects;
using libplctag;
using libplctag.DataTypes;
using RandomTestValues;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace CSharpDotNetCore
{
    class TestDatatypes
    {
        private const int DEFAULT_TIMEOUT = 1000;
        private const string GATEWAY = "10.10.10.10";
        const string PATH = "1,0";
        const PlcType PLC_TYPE = PlcType.ControlLogix;
        const Protocol PROTOCOL = Protocol.ab_eip;


        public static void Run()
        {

            //Bool - Test both cases
            //Random value would look correct 50% of the time
            var boolTag = BuildTag<BoolMarshaller, bool>("TestBOOL");
            TestTag(boolTag, true);
            TestTag(boolTag, false);

            //Signed Numbers
            TestTag(BuildTag<SintMarshaller, sbyte>("TestSINT"));
            TestTag(BuildTag<IntMarshaller, short>("TestINT"));
            TestTag(BuildTag<DintMarshaller, int>("TestDINT"));
            TestTag(BuildTag<LintMarshaller, long>("TestLINT"));

            //Logix doesn't support unsigned

            //Floating Points
            TestTag(BuildTag<RealMarshaller, float>("TestREAL"));
            //TestTag(new GenericTag<PlcTypeLREAL, double>(gateway, Path, PlcType.Logix, "TestLREAL", timeout));

            //Arrays
            //var testArray = new int[] {37, 38, 39, 40, 50 };
            var testArray = RandomValue.Array<int>(5);

            var tagArray = new Tag<DintMarshaller, int[]>()
            {
                Name = "TestDINTArray",
                Gateway = GATEWAY,
                Path = PATH,
                PlcType = PLC_TYPE,
                Protocol = PROTOCOL,
                Timeout = TimeSpan.FromMilliseconds(DEFAULT_TIMEOUT),
                ArrayDimensions = new int[] { 5 },
            };
            tagArray.Initialize();

            TestTag(tagArray, testArray);


        }


        private static bool TestTag<M, T>(Tag<M, T> tag) where T : struct where M : IMarshaller<T>, new()
        {
            T testValue = RandomValue.Object<T>();
            return TestTag(tag, testValue);
        }

        private static bool TestTag<M, T>(Tag<M, T> tag, T testValue) where M : IMarshaller<T>, new()
        {

            Console.WriteLine($"\r\n*** {tag.Name} [{typeof(M)}] {typeof(T)} ***");


            tag.Value = testValue;
            Console.WriteLine($"Write Value <{typeof(T)}> {testValue} to '{tag.Name}'");
            tag.Write();

            Console.WriteLine($"Read Value from {tag.Name}");
            tag.Read();

            T readback = tag.Value;

            CompareLogic compareLogic = new CompareLogic();
            ComparisonResult result = compareLogic.Compare(readback, testValue);

            if (result.AreEqual) Console.WriteLine($"PASS: Read back matched test value");
            else Console.WriteLine($"FAIL: Read back did not match test value - [{readback} != {testValue}]");

            return result.AreEqual;
        }

        private static Tag<M, T> BuildTag<M, T>(string name) where M : IMarshaller<T>, new()
        {
            var tag = new Tag<M, T>()
            {
                Name = name,
                Gateway = GATEWAY,
                Path = PATH,
                PlcType = PLC_TYPE,
                Protocol = PROTOCOL,
                Timeout = TimeSpan.FromMilliseconds(DEFAULT_TIMEOUT),
            };
            tag.Initialize();
            return tag;

        }

    }
}
