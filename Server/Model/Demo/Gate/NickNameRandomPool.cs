using System.Collections.Generic;

namespace ET.Server
{
	[Code]
	public class NickNameRandomPool : Singleton<NickNameRandomPool> ,ISingletonAwake
	{
		/// <summary>性别档位数：0 未指定 / 1 男 / 2 女</summary>
		private const int SexCount = 3;
		
		private NickNameConfigCategory source;
		//姓列表 
		private readonly List<NickNameConfig>[] surnames = new List<NickNameConfig>[SexCount];
		
		//名列表
		private readonly List<NickNameConfig>[] givenNames = new List<NickNameConfig>[SexCount];

		public void Awake()
		{
			this.Build();
		}
		
		public bool TryRandom(int sex, out string nickName)
		{
			nickName = null;
			if (sex < 0 || sex >= SexCount)
			{
				sex = 0;
			}
			
			if (this.source == null || this.source != NickNameConfigCategory.Instance)
			{
				this.Build();
			}

			List<NickNameConfig> surnamePool = this.surnames[sex];
			List<NickNameConfig> givenNamePool = this.givenNames[sex];
			if (surnamePool?.Count == 0 || givenNamePool?.Count == 0)
			{
				return false;
			}
			
			nickName = RandomGenerator.RandomArray(surnamePool).Word + RandomGenerator.RandomArray(givenNamePool).Word;
			return true;
		}
		
		private void Build()
		{
			this.source = NickNameConfigCategory.Instance;
			
			if (this.source == null)
			{
				return;
			}

			for (int sex = 0; sex < SexCount; ++sex)
			{
				this.surnames[sex] = new List<NickNameConfig>();
				this.givenNames[sex] = new List<NickNameConfig>();
			}
			
			List<NickNameConfig> list = this.source.DataList;
			for (int i = 0; i < list.Count; ++i)
			{
				NickNameConfig item = list[i];
				if (item == null || string.IsNullOrEmpty(item.Word))
				{
					continue;
				}

				for (int sex = 0; sex < SexCount; ++sex)
				{
					if (sex != 0 && item.Sex != 0 && item.Sex != sex)
					{
						continue;
					}

					switch (item.Type)
					{
						case 1:
							this.surnames[sex].Add(item);
							break;
						case 2:
							this.givenNames[sex].Add(item);
							break;
					}
				}
			}
		}
	}
}
