CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS noti_applications (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	name TEXT NOT NULL,
	secret TEXT NOT NULL,
	owner_ids TEXT[] NOT NULL DEFAULT '{}',
	is_admin BOOLEAN NOT NULL DEFAULT FALSE,
	
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

	CONSTRAINT noti_applications_unique UNIQUE (name)
);

CREATE TABLE IF NOT EXISTS noti_device_tokens (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	application_id UUID NOT NULL REFERENCES noti_applications(id),
	profile_id TEXT NOT NULL,
	token TEXT NOT NULL,
	name TEXT NOT NULL,
	user_agent TEXT,
	device_type INT NOT NULL DEFAULT 3,
	provider_type INT NOT NULL DEFAULT 1,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

	CONSTRAINT noti_device_tokens_unique UNIQUE (application_id, profile_id, token)
);

CREATE TABLE IF NOT EXISTS noti_topics (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	topic_hash TEXT NOT NULL,
	application_id UUID NOT NULL REFERENCES noti_applications(id),
	
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	
	CONSTRAINT noti_topics_unique UNIQUE (topic_hash, application_id)
);

CREATE TABLE IF NOT EXISTS noti_topic_groups (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	
	application_id UUID NOT NULL REFERENCES noti_applications(id),
	resource_id TEXT NOT NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	
	CONSTRAINT noti_topic_groups_unique UNIQUE (application_id, resource_id)
);

CREATE TABLE IF NOT EXISTS noti_topic_group_map (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	topic_id UUID NOT NULL REFERENCES noti_topics(id),
	group_id UUID NOT NULL REFERENCES noti_topic_groups(id),

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

	CONSTRAINT noti_topic_group_map_unique UNIQUE (topic_id, group_id)
);

CREATE TABLE IF NOT EXISTS noti_topic_group_subscription (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	profile_id TEXT NOT NULL,
	group_id UUID NOT NULL REFERENCES noti_topic_groups(id),

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

	CONSTRAINT noti_topic_group_subscription_unique UNIQUE (profile_id, group_id)
);

CREATE TABLE IF NOT EXISTS noti_topic_subscriptions (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	profile_id TEXT NOT NULL,
	topic_id UUID NOT NULL REFERENCES noti_topics(id),
	group_id UUID REFERENCES noti_topic_groups(id),

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	
	CONSTRAINT noti_topic_subscriptions_unique UNIQUE (profile_id, topic_id)
);

CREATE TABLE IF NOT EXISTS noti_history (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,

	topic_id UUID REFERENCES noti_topics(id),
	profile_id TEXT,
	title TEXT NOT NULL,
	body TEXT NOT NULL,
	data TEXT,
	image_url TEXT,
	results TEXT[] NOT NULL DEFAULT '{}',
	
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);