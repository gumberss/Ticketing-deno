package com.madness.microservice;

import javax.inject.Inject;
import org.eclipse.microprofile.config.ConfigProvider;

import com.madness.microservice.infra.rabbitmq.RabbitMqConnection;
import com.madness.microservice.infra.rabbitmq.events.ProductCreatedEvent;

import io.quarkus.runtime.Quarkus;
import io.quarkus.runtime.QuarkusApplication;
import io.quarkus.runtime.annotations.QuarkusMain;
import com.madness.microservice.infra.rabbitmq.listeners.ProductCreatedListener;


@QuarkusMain
public class Main implements QuarkusApplication {

  @Inject
  public RabbitMqConnection rabbitConnection;

  @Inject
  public ProductCreatedListener productCreatedListener;

  @Override
  public int run(String... args) throws Exception {

    String rabbitMqUri = ConfigProvider.getConfig().getValue("rabbitmq.uri", String.class);
    System.out.println(rabbitMqUri);
    rabbitConnection.withUri(rabbitMqUri).connect();
    productCreatedListener.listen(ProductCreatedEvent.class);

    Quarkus.waitForExit();

    rabbitConnection.channel().close();
    rabbitConnection.connection().close();
    return 0;
  }
}