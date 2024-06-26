int[] nums = new int[] { 4, 1, 2, 1, 2 };
Console.Write(Solution.SingleNumber(nums));
public class Solution
{
    static public int SingleNumber(int[] nums)
    {
        bool result = false;
        int? solution = null;
        nums = nums.Order().ToArray();
        
        int i = 0;
        int j = 1;
        while (result == false && j < nums.Length)
        {
            if (nums[i] != nums[j])
            {
                result = true;
                if (nums[i] < nums[j])
                {
                    solution = nums[i];
                }
                else
                {
                    solution = nums[j];
                }
            }
            i += 2;
            j += 2;
        }

        if(solution == null)
        {
            solution = nums[nums.Length - 1];
        }

        return solution.Value;
    }

    static public bool ContainsDuplicate(int[] nums)
    {
        bool result = false;

        nums = nums.Order().ToArray();

        int i = 0;
        
        while(result == false && i < nums.Length)
        {
            if(i + 1 < nums.Length)
            {
                if (nums[i] == nums[i + 1])
                {
                    result = true;
                }
            }
            i++;
        }

        return result;
    }

    static public void Rotate(int[] nums, int k)
    {
        int n = nums.Length;
        k = k % n;

        if(k % 2 != 0)
        {
            k++;
        }
        int[] temp = new int[k];

        for (int i = 0; i < k; i++)
        {
            temp[i] = nums[i];
        }

        for(int i = k; i < n; i++)
        {
            nums[i - k] = nums[i];
        }

        for(int i = n - k; i < n; i++)
        {
            nums[i] = temp[i - (n - k)];
        }

        for(int i = 0; i < n; i++)
        {
            Console.Write(nums[i]);
        }
    }

    static public int MaxProfit(int[] prices)
    {
        int prof = 0;
        
        for(int i = 1; i < prices.Length; i++)
        {
            if (prices[i] > prices[i - 1])
            {
                prof += prices[i] - prices[i - 1];
            }
        }

        return prof;
    }

    static public int RemoveDuplicates(int[] nums)
    {
        int i = 0;
        int j = 0;
        int ans = 0;
        if(nums.Length > 0)
        {
            ans++;
        }
        while (i < nums.Length)
        {
            int curr = nums[i];
            bool diff = false;
            while (j < nums.Length && !diff)
            {
                if (nums[j] != curr)
                {
                    diff = true;
                    if (i + 1 < nums.Length)
                    {
                        nums[i + 1] = nums[j];
                    }
                    ans++;
                }
                j++;
            }
            i++;
        }

        for(int k = 0; k < nums.Length; k++)
        {
            Console.Write(nums[k]);
        }

        return ans;
    }
}