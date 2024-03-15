using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AudioNoteTranscription.Whisper.ModelConfig
{
    public class ModelAttributes
    {
        public int vocab_size { get; set; }
        public int num_mel_bins { get; set; }
        public int d_model { get; set; }
        public int encoder_layers { get; set; }
        public int encoder_attention_heads { get; set; }
        public int decoder_layers { get; set; }
        public int decoder_attention_heads { get; set; }
        public int decoder_ffn_dim { get; set; }
        public int encoder_ffn_dim { get; set; }
        public double dropout { get; set; }
        public double attention_dropout { get; set; }
        public double activation_dropout { get; set; }
        public string activation_function { get; set; }
        public double init_std { get; set; }
        public double encoder_layerdrop { get; set; }
        public double decoder_layerdrop { get; set; }
        public bool use_cache { get; set; }
        public int num_hidden_layers { get; set; }
        public bool scale_embedding { get; set; }
        public int max_source_positions { get; set; }
        public int max_target_positions { get; set; }
        public int classifier_proj_size { get; set; }
        public bool use_weighted_layer_sum { get; set; }
        public bool apply_spec_augment { get; set; }
        public double mask_time_prob { get; set; }
        public int mask_time_length { get; set; }
        public int mask_time_min_masks { get; set; }
        public double mask_feature_prob { get; set; }
        public int mask_feature_length { get; set; }
        public int mask_feature_min_masks { get; set; }
        public int median_filter_width { get; set; }
        public bool return_dict { get; set; }
        public bool output_hidden_states { get; set; }
        public bool output_attentions { get; set; }
        public bool torchscript { get; set; }
        public string torch_dtype { get; set; }
        public bool use_bfloat16 { get; set; }
        public bool tf_legacy_loss { get; set; }
        public bool tie_word_embeddings { get; set; }
        public bool is_encoder_decoder { get; set; }
        public bool is_decoder { get; set; }
        public object cross_attention_hidden_size { get; set; }
        public bool add_cross_attention { get; set; }
        public bool tie_encoder_decoder { get; set; }
        public int max_length { get; set; }
        public int min_length { get; set; }
        public bool do_sample { get; set; }
        public bool early_stopping { get; set; }
        public int num_beams { get; set; }
        public int num_beam_groups { get; set; }
        public double diversity_penalty { get; set; }
        public double temperature { get; set; }
        public int top_k { get; set; }
        public double top_p { get; set; }
        public double typical_p { get; set; }
        public float repetition_penalty { get; set; }
        public float length_penalty { get; set; }
        public int no_repeat_ngram_size { get; set; }
        public int encoder_no_repeat_ngram_size { get; set; }
        public object bad_words_ids { get; set; }
        public int num_return_sequences { get; set; }
        public int chunk_size_feed_forward { get; set; }
        public bool output_scores { get; set; }
        public bool return_dict_in_generate { get; set; }
        public object forced_bos_token_id { get; set; }
        public object forced_eos_token_id { get; set; }
        public bool remove_invalid_values { get; set; }
        public object exponential_decay_length_penalty { get; set; }
        public List<int> suppress_tokens { get; set; }
        public List<int> begin_suppress_tokens { get; set; }
        public List<string> architectures { get; set; }
        public object finetuning_task { get; set; }
        public object tokenizer_class { get; set; }
        public object prefix { get; set; }
        public int bos_token_id { get; set; }
        public int pad_token_id { get; set; }
        public int eos_token_id { get; set; }
        public object sep_token_id { get; set; }
        public int decoder_start_token_id { get; set; }
        public object task_specific_params { get; set; }
        public object problem_type { get; set; }
        public string transformers_version { get; set; }
        public List<List<int>> forced_decoder_ids { get; set; }
        public string model_type { get; set; }

        [JsonPropertyName("_name_or_path")]
        public string NameOrPath { get; set; }
    }


}
