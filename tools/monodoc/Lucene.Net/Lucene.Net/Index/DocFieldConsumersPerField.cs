/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using Fieldable = Mono.Lucene.Net.Documents.Fieldable;

namespace Mono.Lucene.Net.Index
{
	
	sealed class DocFieldConsumersPerField:DocFieldConsumerPerField
	{
		
		internal DocFieldConsumerPerField one;
		internal DocFieldConsumerPerField two;
		internal DocFieldConsumersPerThread perThread;
		
		public DocFieldConsumersPerField(DocFieldConsumersPerThread perThread, DocFieldConsumerPerField one, DocFieldConsumerPerField two)
		{
			this.perThread = perThread;
			this.one = one;
			this.two = two;
		}
		
		public override void  ProcessFields(Fieldable[] fields, int count)
		{
			one.ProcessFields(fields, count);
			two.ProcessFields(fields, count);
		}
		
		public override void  Abort()
		{
			try
			{
				one.Abort();
			}
			finally
			{
				two.Abort();
			}
		}
	}
}
